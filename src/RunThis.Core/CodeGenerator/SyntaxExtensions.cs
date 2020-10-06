using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RunThis.Core.CodeGenerator
{
    public static class SyntaxExtensions
    {
        public static TypeSyntax ToTypeSyntax(this Type type)
        {
            //return SyntaxFactory.ParseTypeName(type.FullName);
            return
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier(SyntaxTriviaList.Empty, type.FullName, SyntaxTriviaList.Empty)
                );
        }

        public static SyntaxToken ToIdentifier(this string identifier)
        {
            identifier = identifier.TrimStart('@');

            if (Identifier.IsCSharpKeyword(identifier))
            {
                return SyntaxFactory.VerbatimIdentifier(
                    SyntaxTriviaList.Empty,
                    identifier,
                    identifier,
                    SyntaxTriviaList.Empty);
            }

            return SyntaxFactory.Identifier(SyntaxTriviaList.Empty, identifier, SyntaxTriviaList.Empty);
        }




        private const char genericSpecialChar = '`';
        private const string genericSeparator = ", ";

        public static string GetCleanName(this Type t)
        {
            string name = t.Name;
            if (t.IsGenericType)
            {
                name = name.Remove(name.IndexOf(genericSpecialChar));
            }
            return name;
        }

        public static string GetFriendlyFullName(this Type t, out string[] genericParameters)
        {
            genericParameters = null;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}.{1}", t.Namespace, t.GetCleanName());
            if (t.IsGenericType)
            {
                genericParameters = (from ga in t.GetGenericArguments()
                                     select GetFriendlyFullName(ga, out _)).ToArray();

                sb.Append("<");
                sb.Append(string.Join(genericSeparator, genericParameters));
                sb.Append(">");
            }
            return sb.ToString();
        }

        public static string GetFriendlyName(this Type t, out string[] genericParameters)
        {
            genericParameters = null;
            StringBuilder sb = new StringBuilder();
            sb.Append(t.GetCleanName());
            if (t.IsGenericType)
            {
                genericParameters = (from ga in t.GetGenericArguments()
                                     select GetFriendlyFullName(ga, out _)).ToArray();

                sb.Append("<");
                sb.Append(string.Join(genericSeparator, genericParameters));
                sb.Append(">");
            }
            return sb.ToString();
        }



    }
}
