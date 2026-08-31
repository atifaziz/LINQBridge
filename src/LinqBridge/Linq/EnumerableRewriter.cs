// Decompiled with JetBrains decompiler
// Type: System.Linq.EnumerableRewriter
// Assembly: System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: CF2EEE1F-04F2-4122-84EA-132765256A1E
// Assembly location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.xml

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    internal class EnumerableRewriter : ExpressionVisitor
    {
        private static ILookup<string, MethodInfo> _seqMethods;

        internal EnumerableRewriter()
        {
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression instance = this.Visit(m.Object);
            ReadOnlyCollection<Expression> readOnlyCollection = this.VisitExpressionList(m.Arguments);
            if (instance == m.Object && readOnlyCollection == m.Arguments)
                return (Expression)m;
            readOnlyCollection.ToArray<Expression>();
            Type[] typeArgs = m.Method.IsGenericMethod ? m.Method.GetGenericArguments() : (Type[])null;
            if ((m.Method.IsStatic || m.Method.DeclaringType.IsAssignableFrom(instance.Type)) && EnumerableRewriter.ArgsMatch(m.Method, readOnlyCollection, typeArgs))
                return (Expression)Expression.Call(instance, m.Method, (IEnumerable<Expression>)readOnlyCollection);
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                MethodInfo enumerableMethod = EnumerableRewriter.FindEnumerableMethod(m.Method.Name, readOnlyCollection, typeArgs);
                ReadOnlyCollection<Expression> arguments = this.FixupQuotedArgs(enumerableMethod, readOnlyCollection);
                return (Expression)Expression.Call(instance, enumerableMethod, (IEnumerable<Expression>)arguments);
            }
            BindingFlags flags = (BindingFlags)(8 | (m.Method.IsPublic ? 16 : 32));
            MethodInfo method = EnumerableRewriter.FindMethod(m.Method.DeclaringType, m.Method.Name, readOnlyCollection, typeArgs, flags);
            ReadOnlyCollection<Expression> arguments1 = this.FixupQuotedArgs(method, readOnlyCollection);
            return (Expression)Expression.Call(instance, method, (IEnumerable<Expression>)arguments1);
        }

        private ReadOnlyCollection<Expression> FixupQuotedArgs(
          MethodInfo mi,
          ReadOnlyCollection<Expression> argList)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length > 0)
            {
                List<Expression> sequence = (List<Expression>)null;
                int index1 = 0;
                for (int length = parameters.Length; index1 < length; ++index1)
                {
                    Expression expression1 = argList[index1];
                    Expression expression2 = this.FixupQuotedExpression(parameters[index1].ParameterType, expression1);
                    if (sequence == null && expression2 != argList[index1])
                    {
                        sequence = new List<Expression>(argList.Count);
                        for (int index2 = 0; index2 < index1; ++index2)
                            sequence.Add(argList[index2]);
                    }
                    sequence?.Add(expression2);
                }
                if (sequence != null)
                    argList = sequence.ToReadOnlyCollection<Expression>();
            }
            return argList;
        }

        private Expression FixupQuotedExpression(Type type, Expression expression)
        {
            Expression expression1;
            for (expression1 = expression; !type.IsAssignableFrom(expression1.Type); expression1 = ((UnaryExpression)expression1).Operand)
            {
                if (expression1.NodeType != ExpressionType.Quote)
                {
                    if (!type.IsAssignableFrom(expression1.Type) && type.IsArray && expression1.NodeType == ExpressionType.NewArrayInit)
                    {
                        Type c = EnumerableRewriter.StripExpression(expression1.Type);
                        if (type.IsAssignableFrom(c))
                        {
                            Type elementType = type.GetElementType();
                            NewArrayExpression newArrayExpression = (NewArrayExpression)expression1;
                            List<Expression> initializers = new List<Expression>(newArrayExpression.Expressions.Count);
                            int index = 0;
                            for (int count = newArrayExpression.Expressions.Count; index < count; ++index)
                                initializers.Add(this.FixupQuotedExpression(elementType, newArrayExpression.Expressions[index]));
                            expression = (Expression)Expression.NewArrayInit(elementType, (IEnumerable<Expression>)initializers);
                        }
                    }
                    return expression;
                }
            }
            return expression1;
        }

        internal override Expression VisitLambda(LambdaExpression lambda) => (Expression)lambda;

        private static Type GetPublicType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Lookup<,>.Grouping))
                return typeof(IGrouping<,>).MakeGenericType(t.GetGenericArguments());
            if (!t.IsNestedPrivate)
                return t;
            foreach (Type publicType in t.GetInterfaces())
            {
                if (publicType.IsGenericType && publicType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return publicType;
            }
            return typeof(IEnumerable).IsAssignableFrom(t) ? typeof(IEnumerable) : t;
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            if (!(c.Value is EnumerableQuery enumerableQuery))
                return (Expression)c;
            if (enumerableQuery.Enumerable == null)
                return this.Visit(enumerableQuery.Expression);
            Type publicType = EnumerableRewriter.GetPublicType(enumerableQuery.Enumerable.GetType());
            return (Expression)Expression.Constant((object)enumerableQuery.Enumerable, publicType);
        }

        internal override Expression VisitParameter(ParameterExpression p) => (Expression)p;

        private static MethodInfo FindEnumerableMethod(
          string name,
          ReadOnlyCollection<Expression> args,
          params Type[] typeArgs)
        {
            if (EnumerableRewriter._seqMethods == null)
                EnumerableRewriter._seqMethods = ((IEnumerable<MethodInfo>)typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)).ToLookup<MethodInfo, string>((Func<MethodInfo, string>)(m => m.Name));
            MethodInfo methodInfo = EnumerableRewriter._seqMethods[name].FirstOrDefault<MethodInfo>((Func<MethodInfo, bool>)(m => EnumerableRewriter.ArgsMatch(m, args, typeArgs)));
            if (methodInfo == null)
                throw new ArgumentException(name);
            return typeArgs != null ? methodInfo.MakeGenericMethod(typeArgs) : methodInfo;
        }

        internal static MethodInfo FindMethod(
          Type type,
          string name,
          ReadOnlyCollection<Expression> args,
          Type[] typeArgs,
          BindingFlags flags)
        {
            MethodInfo[] array = ((IEnumerable<MethodInfo>)type.GetMethods(flags)).Where<MethodInfo>((Func<MethodInfo, bool>)(m => m.Name == name)).ToArray<MethodInfo>();
            if (array.Length == 0)
                throw new ArgumentException(name);
            MethodInfo methodInfo = ((IEnumerable<MethodInfo>)array).FirstOrDefault<MethodInfo>((Func<MethodInfo, bool>)(m => EnumerableRewriter.ArgsMatch(m, args, typeArgs)));
            if (methodInfo == null)
                throw new ArgumentException(name);
            return typeArgs != null ? methodInfo.MakeGenericMethod(typeArgs) : methodInfo;
        }

        private static bool ArgsMatch(
          MethodInfo m,
          ReadOnlyCollection<Expression> args,
          Type[] typeArgs)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (parameters.Length != args.Count || !m.IsGenericMethod && typeArgs != null && typeArgs.Length > 0)
                return false;
            if (!m.IsGenericMethodDefinition && m.IsGenericMethod && m.ContainsGenericParameters)
                m = m.GetGenericMethodDefinition();
            if (m.IsGenericMethodDefinition)
            {
                if (typeArgs == null || typeArgs.Length == 0 || m.GetGenericArguments().Length != typeArgs.Length)
                    return false;
                m = m.MakeGenericMethod(typeArgs);
                parameters = m.GetParameters();
            }
            int index = 0;
            for (int count = args.Count; index < count; ++index)
            {
                Type type = parameters[index].ParameterType;
                if (type == null)
                    return false;
                if (type.IsByRef)
                    type = type.GetElementType();
                Expression operand = args[index];
                if (!type.IsAssignableFrom(operand.Type))
                {
                    if (operand.NodeType == ExpressionType.Quote)
                        operand = ((UnaryExpression)operand).Operand;
                    if (!type.IsAssignableFrom(operand.Type) && !type.IsAssignableFrom(EnumerableRewriter.StripExpression(operand.Type)))
                        return false;
                }
            }
            return true;
        }

        private static Type StripExpression(Type type)
        {
            bool isArray = type.IsArray;
            Type type1 = isArray ? type.GetElementType() : type;
            Type genericType = TypeHelper.FindGenericType(typeof(Expression<>), type1);
            if (genericType != null)
                type1 = genericType.GetGenericArguments()[0];
            if (!isArray)
                return type;
            int arrayRank = type.GetArrayRank();
            return arrayRank != 1 ? type1.MakeArrayType(arrayRank) : type1.MakeArrayType();
        }
    }
}
