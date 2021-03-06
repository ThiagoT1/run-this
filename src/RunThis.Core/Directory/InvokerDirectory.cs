using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using RunThis.Core.CodeGenerator;
using RunThis.Core.Invoker;

namespace RunThis.Core.Directory
{


    public interface IInvokerDirectory
    {
        T AsAddress<T>(T target);
        bool DeactivateAddress<T>(T target);
    }
    public class InvokerDirectory : IInvokerDirectory
    {
        class InvokerContext
        {
            ConcurrentDictionary<Type, object> _proxies;
            IInvoker _invoker;
            public InvokerContext()
            {
                _proxies = new ConcurrentDictionary<Type, object>();
                _invoker = new SingleThreadInvoker();
            }

            internal T GetProxy<T>(T target, ILogger<InvokerDirectory> logger)
            {
                var invoker = _invoker;
                var localTarget = target;
                var localLogger = logger;

                var proxy = _proxies.GetOrAdd(typeof(T), (type) => ProxyCache<T>.CreateProxy(localTarget, invoker, localLogger));

                return (T)proxy;
            }
        }

        private readonly ConcurrentDictionary<object, InvokerContext> _contexts;
        private readonly ILogger<InvokerDirectory> _logger;

        public InvokerDirectory(ILogger<InvokerDirectory> logger)
        {
            _contexts = new ConcurrentDictionary<object, InvokerContext>();
            _logger = logger;
        }

        static class ProxyCache<T>
        {
            static Func<T, IInvoker, ILogger, T> _proxyFactory;
            static object _lockObject;

            static ProxyCache()
            {
                if (!typeof(T).IsInterface)
                    throw new Exception("We need interfaces");

                _lockObject = new object();
            }
            internal static T CreateProxy(T target, IInvoker invoker, ILogger logger)
            {
                if (_proxyFactory == null)
                    lock (_lockObject)
                        if (_proxyFactory == null)
                            _proxyFactory = CreateProxyFactory(logger);

                return _proxyFactory(target, invoker, logger);
            }

            private static Func<T, IInvoker, ILogger, T> CreateProxyFactory(ILogger logger)
            {
                var source = CreateProxyCode(out var typeName);

                if (!TryCompileType(source, logger, typeName, out var proxyType, out var message))
                    throw new Exception(message);

                return (T target, IInvoker invoker, ILogger logger) => (T)Activator.CreateInstance(proxyType, target, invoker);
            }




            private static string CreateProxyCode(out string typeName)
            {
                var interfaceType = typeof(T);

                typeName = $"RunThis.Autogenerated.Proxy.{typeof(T).Namespace}.{interfaceType.GetProxyName(out _)}Proxy";

                var methodCalls = new StringBuilder();
                WriteMethodCalls(methodCalls, interfaceType);

                const string template = @"
                    
                    namespace RunThis.Autogenerated.Proxy.{3}
                    {{
                        //TName: {0}
                        //TFullName: {1}
                        //CallStructs: {{2}}                        
                        public class {0}Proxy : {1}
                        {{
                            private readonly {1} _target;
                            private readonly global::RunThis.Core.Invoker.IInvoker _invoker;


                            public {0}Proxy({1} target, global::RunThis.Core.Invoker.IInvoker invoker)
                            {{
                                _target = target;
                                _invoker = invoker;
                            }}

                            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                            private global::System.Threading.Tasks.ValueTask ExecuteVoidCall(global::RunThis.Core.Invoker.ICall call)
                            {{
                                return _invoker.ExecuteVoidCall(call);
                            }}

                            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                            private global::System.Threading.Tasks.ValueTask<T> ExecuteValueCall<T>(global::RunThis.Core.Invoker.ICall<T> call)
                            {{
                                return _invoker.ExecuteValueCall(call);
                            }}

                            {2}

                        }}
                    }}
                ";



                return string.Format(
                    template,
                    typeof(T).GetProxyName(out _),
                    typeof(T).GetFriendlyGlobalName(out _),
                    methodCalls.ToString(),
                    typeof(T).Namespace
                );
            }

            readonly struct ParameterInfo
            {
                public readonly string FullTypeName;
                public readonly string ParemeterName;

                public ParameterInfo(string fullTypeName, string paremeterName)
                {
                    FullTypeName = fullTypeName;
                    ParemeterName = paremeterName;
                }
            }
            private static void GetInterfaceMethods(List<MethodInfo> methodList, Type type)
            {
                methodList.AddRange(type.GetMethods());
                foreach (var innerInterface in type.GetInterfaces())
                    GetInterfaceMethods(methodList, innerInterface);
            }
            private static void WriteMethodCalls(StringBuilder methodCalls, Type interfaceType)
            {
                List<MethodInfo> methods = new List<MethodInfo>();

                GetInterfaceMethods(methods, interfaceType);

                const string voidCallTemplate = @"

                            public {0} {1}({2})
                            {{
                                return ExecuteVoidCall(new {1}Call(_target{4}{3}));
                            }}

                ";

                const string valueCallTemplate = @"

                            public {0} {1}({2})
                            {{
                                return ExecuteValueCall(new {1}Call(_target{4}{3}));
                            }}

                ";


                const string structTemplate = @"

                            readonly struct {1}Call : global::RunThis.Core.Invoker.ICall{7}
                            {{
                                private readonly {0} _target;
                                
                                {5}

                                public {1}Call({0} target{8}{3})
                                {{
                                    _target = target;
                                    {6}
                                }}

                                public {2} Invoke() => _target.{1}({4});
                            }}

                ";

                const string privateDeclarationTemplate = @"
                                private readonly {0} {1};
                ";

                const string privateStoreTemplate = @"
                                    this.{0} = {0};
                ";

                foreach (var method in methods)
                {
                    ValidateReturnType(method.ReturnType, out bool isGeneric, out var returnTypefullName, out var returnGenericParameters);

                    string returnParameterFullName = null;

                    if (returnGenericParameters != null)
                        returnParameterFullName = $"<{string.Join(", ", returnGenericParameters)}>";

                    string template = null;

                    if (isGeneric)
                    {
                        template = valueCallTemplate;
                    }
                    else
                    {
                        template = voidCallTemplate;
                    }

                    var parameters = method.GetParameters().Select(p => new ParameterInfo(p.ParameterType.GetFriendlyGlobalName(out _), p.Name)).ToArray();

                    var parameterList = string.Join(", ", parameters.Select(x => $"{x.FullTypeName} {x.ParemeterName}"));
                    var parameterNames = string.Join(", ", parameters.Select(x => $"{x.ParemeterName}"));

                    var privateFields = parameters.Select(x => string.Format(privateDeclarationTemplate, x.FullTypeName, x.ParemeterName));
                    var storeFields = parameters.Select(x => string.Format(privateStoreTemplate, x.ParemeterName));

                    methodCalls.AppendFormat(template, $"{returnTypefullName}", method.Name, parameterList, parameterNames, parameterList.Length > 0 ? ", " : "");

                    template = structTemplate;

                    methodCalls.AppendFormat(
                        template,
                        interfaceType.GetFriendlyGlobalName(out _),   //0
                        method.Name,                                //1
                        returnTypefullName,                         //2
                        parameterList,                              //3
                        parameterNames,                             //4
                        string.Join("", privateFields),             //5
                        string.Join("", storeFields),               //6
                        returnParameterFullName,                    //7
                        parameterList.Length > 0 ? ", " : ""        //8
                    );


                }
            }

            private static void ValidateReturnType(Type returnType, out bool isGeneric, out string fullName,
                out string[] returnGenericParameters)
            {
                isGeneric = false;
                fullName = returnType.FullName;

                if (returnType == typeof(ValueTask))
                {
                    fullName = returnType.GetFriendlyGlobalName(out returnGenericParameters);
                    returnGenericParameters = null;
                    return;
                }

                if (returnType.IsGenericType)
                {
                    fullName = returnType.GetFriendlyGlobalName(out returnGenericParameters);
                    isGeneric = true;
                    if (returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                        return;
                }

                throw new Exception($"[{typeof(T).GetFriendlyFullName(out _)}] Return type must be either {nameof(ValueTask)} or {nameof(ValueTask)}<T> (it was {fullName})");
            }

            private static bool TryCompileType(string source, ILogger logger, string typeName, out Type type, out string message)
            {
                message = "";
                type = null;
                logger?.LogInformation($"[{typeof(T).GetFriendlyFullName(out _)}] Parsing the code into the SyntaxTree");


                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);


                string assemblyName = Path.GetRandomFileName();

                var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

                List<PortableExecutableReference> references = new List<PortableExecutableReference>();

                references.AddRange(
                    typeof(T).Assembly
                    .GetReferencedAssemblies()
                    .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
                );

                references.Add(MetadataReference.CreateFromFile(typeof(T).Assembly.Location));

                references.AddRange(
                    trustedAssembliesPaths
                    .Where(x => x.ToLower().Contains("netstandard") || x.ToLower().Contains("runtime"))
                    .Select(x => MetadataReference.CreateFromFile(x))
                );

                references.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));

                logger?.LogInformation($"[{typeof(T).GetFriendlyFullName(out _)}] Adding the following references");

                foreach (var r in references)
                    logger?.LogInformation($"[{typeof(T).GetFriendlyFullName(out _)}] {r.FilePath}");

                logger?.LogInformation($"[{typeof(T).GetFriendlyFullName(out _)}] Compiling ...");

                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        logger?.LogError($"[{typeof(T).GetFriendlyFullName(out _)}] Compilation failed!");
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);

                        foreach (Diagnostic diagnostic in failures)
                            message += $"[{typeof(T).GetFriendlyFullName(out _)}] {diagnostic.Id}: {diagnostic.GetMessage()}\n";

                        return false;
                    }
                    else
                    {
                        logger?.LogInformation($"[{typeof(T).GetFriendlyFullName(out _)}] Compilation successful!");
                        ms.Seek(0, SeekOrigin.Begin);

                        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                        type = assembly.GetType(typeName);

                        logger?.LogInformation($"[{typeof(T).GetFriendlyFullName(out _)}] Compiled type => {type.GetFriendlyName(out _)}");

                        return true;
                    }
                }
            }

        }

        private InvokerContext CreateContext()
        {
            return new InvokerContext();
        }

        public T AsAddress<T>(T target)
        {
            var context = _contexts.GetOrAdd(target, (obj) => CreateContext());
            return context.GetProxy(target, _logger);
        }

        public bool DeactivateAddress<T>(T target)
        {
            return _contexts.TryRemove(target, out _);
        }

    }
}
