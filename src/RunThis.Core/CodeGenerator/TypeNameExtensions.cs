using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RunThis.Core.CodeGenerator
{
    public static class TypeNameExtensions
    {

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
