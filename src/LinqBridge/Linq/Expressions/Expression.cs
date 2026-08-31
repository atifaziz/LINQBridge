using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents a strongly typed lambda expression as a data structure in the form of an expression tree. This class cannot be inherited.</summary>
    /// <typeparam name="TDelegate">The type of the delegate that the <see cref="T:System.Linq.Expressions.Expression`1" /> represents.</typeparam>
    public sealed class Expression<TDelegate> : LambdaExpression
    {
        internal Expression(Expression body, ReadOnlyCollection<ParameterExpression> parameters)
          : base(body, typeof(TDelegate), parameters)
        {
        }

        /// <summary>Compiles the lambda expression described by the expression tree into executable code.</summary>
        /// <returns>A delegate of type <paramref name="TDelegate" /> that represents the lambda expression described by the <see cref="T:System.Linq.Expressions.Expression`1" />.</returns>
        public TDelegate Compile()
        {
            return (TDelegate)((object)base.Compile());
        }
    }

    /// <summary>Provides the base class from which the classes that represent expression tree nodes are derived. It also contains static (Shared in Visual Basic) factory methods to create the various node types. This is an abstract class.</summary>
    public abstract class Expression
    {
        private ExpressionType nodeType;
        private Type type;
        private static Type[] lambdaTypes = new Type[2]
        {
      typeof (Expression),
      typeof (IEnumerable<ParameterExpression>)
        };
        private static readonly Type[] funcTypes = new Type[5]
        {
      typeof (Func<>),
      typeof (Func<,>),
      typeof (Func<,,>),
      typeof (Func<,,,>),
      typeof (Func<,,,,>)
        };
        private static readonly Type[] actionTypes = new Type[5]
        {
      typeof (Action),
      typeof (Action<>),
      typeof (Action<,>),
      typeof (Action<,,>),
      typeof (Action<,,,>)
        };

        /// <summary>Initializes a new instance of the <see cref="T:System.Linq.Expressions.Expression" /> class.</summary>
        /// <param name="nodeType">The <see cref="T:System.Linq.Expressions.ExpressionType" /> to set as the node type.</param>
        /// <param name="type">The <see cref="T:System.Type" /> to set as the type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</param>
        protected Expression(ExpressionType nodeType, Type type)
        {
            this.nodeType = nodeType;
            this.type = type;
        }

        [__DynamicallyInvokable]
        protected Expression()
        {
        }

        /// <summary>Gets the node type of this <see cref="T:System.Linq.Expressions.Expression" />.</summary>
        /// <returns>One of the <see cref="T:System.Linq.Expressions.ExpressionType" /> values.</returns>
        public virtual ExpressionType NodeType => this.nodeType;

        /// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
        /// <returns>The <see cref="T:System.Type" /> that represents the static type of the expression.</returns>
        public virtual Type Type => this.type;

        public static BinaryExpression Assign(Expression left, Expression right)
        {
            Expression.RequiresCanWrite(left, "left");
            Expression.RequiresCanRead(right, "right");
            TypeUtils.ValidateType(left.Type);
            TypeUtils.ValidateType(right.Type);
            if (!TypeUtils.AreReferenceAssignable(left.Type, right.Type))
            {
                throw new ArgumentException($"{right.Type}!={left.Type}");
            }
            return new AssignBinaryExpression(left, right);
        }

        private static void RequiresCanRead(Expression expression, string paramName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(paramName);
            }
            ExpressionType nodeType = expression.NodeType;
            if (nodeType == ExpressionType.MemberAccess)
            {
                MemberInfo member = ((MemberExpression)expression).Member;
                if (member.MemberType == MemberTypes.Property && !((PropertyInfo)member).CanRead)
                {
                    throw new ArgumentException(paramName);
                }
            }
            else if (nodeType == ExpressionType.Index)
            {
                IndexExpression indexExpression = (IndexExpression)expression;
                if (indexExpression.Indexer != null && !indexExpression.Indexer.CanRead)
                {
                    throw new ArgumentException(paramName);
                }
            }
        }

        private static void RequiresCanRead(IEnumerable<Expression> items, string paramName)
        {
            if (items != null)
            {
                IList<Expression> expressions = items as IList<Expression>;
                if (expressions != null)
                {
                    for (int i = 0; i < expressions.Count; i++)
                    {
                        Expression.RequiresCanRead(expressions[i], paramName);
                    }
                    return;
                }
                foreach (Expression item in items)
                {
                    Expression.RequiresCanRead(item, paramName);
                }
            }
        }

        private static void RequiresCanWrite(Expression expression, string paramName)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(paramName);
            }
            bool canWrite = false;
            ExpressionType nodeType = expression.NodeType;
            if (nodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExpression = (MemberExpression)expression;
                MemberTypes memberType = memberExpression.Member.MemberType;
                if (memberType == MemberTypes.Field)
                {
                    FieldInfo member = (FieldInfo)memberExpression.Member;
                    canWrite = (member.IsInitOnly ? false : !member.IsLiteral);
                }
                else if (memberType == MemberTypes.Property)
                {
                    canWrite = ((PropertyInfo)memberExpression.Member).CanWrite;
                }
            }
            else if (nodeType == ExpressionType.Parameter)
            {
                canWrite = true;
            }
            else if (nodeType == ExpressionType.Index)
            {
                IndexExpression indexExpression = (IndexExpression)expression;
                canWrite = (indexExpression.Indexer == null ? true : indexExpression.Indexer.CanWrite);
            }
            if (!canWrite)
            {
                throw new ArgumentException(paramName);
            }
        }
        /// <summary>Returns a textual representation of the <see cref="T:System.Linq.Expressions.Expression" />.</summary>
        /// <returns>A textual representation of the <see cref="T:System.Linq.Expressions.Expression" />.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.BuildString(builder);
            return builder.ToString();
        }

        internal virtual void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            builder.Append("[");
            builder.Append(this.nodeType.ToString());
            builder.Append("]");
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic addition operation that does not have overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Add" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The addition operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Add(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.Add, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Add, "op_Addition", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic addition operation that does not have overflow checking. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Add" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the addition operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Add(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Add(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Add, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic addition operation that has overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.AddChecked" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The addition operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression AddChecked(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.AddChecked, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.AddChecked, "op_Addition", left, right, false);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic addition operation that has overflow checking. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.AddChecked" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the addition operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression AddChecked(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.AddChecked(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.AddChecked, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise AND operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.And" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The bitwise AND operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression And(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsIntegerOrBool(left.Type) ? new BinaryExpression(ExpressionType.And, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.And, "op_BitwiseAnd", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise AND operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.And" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the bitwise AND operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression And(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.And(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.And, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a conditional AND operation that evaluates the second operand only if it has to.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.AndAlso" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The bitwise AND operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.-or-<paramref name="left" />.Type and <paramref name="right" />.Type are not the same Boolean type.</exception>
        public static BinaryExpression AndAlso(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            if (left.Type == right.Type && Expression.IsBool(left.Type))
                return new BinaryExpression(ExpressionType.AndAlso, left, right, left.Type);
            MethodInfo definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(ExpressionType.AndAlso, left.Type, right.Type, "op_BitwiseAnd");
            if (definedBinaryOperator == null)
                throw Error.BinaryOperatorNotDefined((object)ExpressionType.AndAlso, (object)left.Type, (object)right.Type);
            Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, definedBinaryOperator);
            Type type = !Expression.IsNullableType(left.Type) || definedBinaryOperator.ReturnType != Expression.GetNonNullableType(left.Type) ? definedBinaryOperator.ReturnType : left.Type;
            return new BinaryExpression(ExpressionType.AndAlso, left, right, definedBinaryOperator, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a conditional AND operation that evaluates the second operand only if it has to. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.AndAlso" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the bitwise AND operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.-or-<paramref name="method" /> is null and <paramref name="left" />.Type and <paramref name="right" />.Type are not the same Boolean type.</exception>
        public static BinaryExpression AndAlso(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            if (method == null)
                return Expression.AndAlso(left, right);
            Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.Type, right.Type, method);
            Type type = !Expression.IsNullableType(left.Type) || method.ReturnType != Expression.GetNonNullableType(left.Type) ? method.ReturnType : left.Type;
            return new BinaryExpression(ExpressionType.AndAlso, left, right, method, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents applying an array index operator to an array of rank one.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ArrayIndex" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="array">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="index">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> or <paramref name="index" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="array" />.Type does not represent an array type.-or-<paramref name="array" />.Type represents an array type whose rank is not 1.-or-<paramref name="index" />.Type does not represent the <see cref="T:System.Int32" /> type.</exception>
        public static BinaryExpression ArrayIndex(Expression array, Expression index)
        {
            if (array == null)
                throw Error.ArgumentNull(nameof(array));
            if (index == null)
                throw Error.ArgumentNull(nameof(index));
            if (index.Type != typeof(int))
                throw Error.ArgumentMustBeArrayIndexType();
            if (!array.Type.IsArray)
                throw Error.ArgumentMustBeArray();
            if (array.Type.GetArrayRank() != 1)
                throw Error.IncorrectNumberOfIndexes();
            return new BinaryExpression(ExpressionType.ArrayIndex, array, index, array.Type.GetElementType());
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents applying an array index operator to an array of rank more than one.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" /> and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> and <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="array">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> property equal to.</param>
        /// <param name="indexes">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> or <paramref name="indexes" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="array" />.Type does not represent an array type.-or-The rank of <paramref name="array" />.Type does not match the number of elements in <paramref name="indexes" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of one or more elements of <paramref name="indexes" /> does not represent the <see cref="T:System.Int32" /> type.</exception>
        public static MethodCallExpression ArrayIndex(
          Expression array,
          params Expression[] indexes)
        {
            return Expression.ArrayIndex(array, (IEnumerable<Expression>)indexes);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents applying an array index operator to an array of rank more than one.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" /> and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> and <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="array">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> property equal to.</param>
        /// <param name="indexes">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> or <paramref name="indexes" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="array" />.Type does not represent an array type.-or-The rank of <paramref name="array" />.Type does not match the number of elements in <paramref name="indexes" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of one or more elements of <paramref name="indexes" /> does not represent the <see cref="T:System.Int32" /> type.</exception>
        public static MethodCallExpression ArrayIndex(
          Expression array,
          IEnumerable<Expression> indexes)
        {
            if (array == null)
                throw Error.ArgumentNull(nameof(array));
            if (indexes == null)
                throw Error.ArgumentNull(nameof(indexes));
            if (!array.Type.IsArray)
                throw Error.ArgumentMustBeArray();
            ReadOnlyCollection<Expression> readOnlyCollection = indexes.ToReadOnlyCollection<Expression>();
            if (array.Type.GetArrayRank() != readOnlyCollection.Count)
                throw Error.IncorrectNumberOfIndexes();
            foreach (Expression expression in readOnlyCollection)
            {
                if (expression.Type != typeof(int))
                    throw Error.ArgumentMustBeArrayIndexType();
            }
            MethodInfo method = array.Type.GetMethod("Get", BindingFlags.Instance | BindingFlags.Public);
            return Expression.Call(array, method, (IEnumerable<Expression>)readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents getting the length of a one-dimensional array.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ArrayLength" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to <paramref name="array" />.</returns>
        /// <param name="array">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="array" />.Type does not represent an array type.</exception>
        public static UnaryExpression ArrayLength(Expression array)
        {
            if (array == null)
                throw Error.ArgumentNull(nameof(array));
            if (!array.Type.IsArray || !Expression.AreAssignable(typeof(Array), array.Type))
                throw Error.ArgumentMustBeArray();
            return array.Type.GetArrayRank() == 1 ? new UnaryExpression(ExpressionType.ArrayLength, array, typeof(int)) : throw Error.ArgumentMustBeSingleDimensionalArrayType();
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberAssignment" /> that represents the initialization of a field or property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberAssignment" /> that has <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> equal to <see cref="F:System.Linq.Expressions.MemberBindingType.Assignment" /> and the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> and <see cref="P:System.Linq.Expressions.MemberAssignment.Expression" /> properties set to the specified values.</returns>
        /// <param name="member">A <see cref="T:System.Reflection.MemberInfo" /> to set the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property equal to.</param>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MemberAssignment.Expression" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="member" /> or <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="member" /> does not represent a field or property.-or-The property represented by <paramref name="member" /> does not have a set accessor.-or-<paramref name="expression" />.Type is not assignable to the type of the field or property that <paramref name="member" /> represents.</exception>
        public static MemberAssignment Bind(MemberInfo member, Expression expression)
        {
            if (member == null)
                throw Error.ArgumentNull(nameof(member));
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            Type memberType;
            Expression.ValidateSettableFieldOrPropertyMember(member, out memberType);
            if (!Expression.AreAssignable(memberType, expression.Type))
                throw Error.ArgumentTypesMustMatch();
            return new MemberAssignment(member, expression);
        }

        private static PropertyInfo GetProperty(MethodInfo mi)
        {
            foreach (PropertyInfo property in mi.DeclaringType.GetProperties((BindingFlags)(48 | (mi.IsStatic ? 8 : 4))))
            {
                if (property.CanRead && Expression.CheckMethod(mi, property.GetGetMethod(true)) || property.CanWrite && Expression.CheckMethod(mi, property.GetSetMethod(true)))
                    return property;
            }
            throw Error.MethodNotPropertyAccessor((object)mi.DeclaringType, (object)mi.Name);
        }

        private static bool CheckMethod(MethodInfo method, MethodInfo propertyMethod)
        {
            if (method == propertyMethod)
                return true;
            Type declaringType = method.DeclaringType;
            return declaringType.IsInterface && method.Name == propertyMethod.Name && declaringType.GetMethod(method.Name) == propertyMethod;
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberAssignment" /> that represents the initialization of a member by using a property accessor method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberAssignment" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.Assignment" />, the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property set to the <see cref="T:System.Reflection.PropertyInfo" /> that represents the property accessed in <paramref name="propertyAccessor" />, and the <see cref="P:System.Linq.Expressions.MemberAssignment.Expression" /> property set to <paramref name="expression" />.</returns>
        /// <param name="propertyAccessor">A <see cref="T:System.Reflection.MethodInfo" /> that represents a property accessor method.</param>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MemberAssignment.Expression" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="propertyAccessor" /> or <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="propertyAccessor" /> does not represent a property accessor method.-or-The property accessed by <paramref name="propertyAccessor" /> does not have a set accessor.-or-<paramref name="expression" />.Type is not assignable to the type of the field or property that <paramref name="member" /> represents.</exception>
        public static MemberAssignment Bind(
          MethodInfo propertyAccessor,
          Expression expression)
        {
            if (propertyAccessor == null)
                throw Error.ArgumentNull(nameof(propertyAccessor));
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            Expression.ValidateMethodInfo(propertyAccessor);
            return Expression.Bind((MemberInfo)Expression.GetProperty(propertyAccessor), expression);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Expression arg0, Expression arg1)
        {
            Expression.RequiresCanRead(arg0, "arg0");
            Expression.RequiresCanRead(arg1, "arg1");
            return new Block2(arg0, arg1);
        }

        internal static T ReturnObject<T>(object collectionOrT)
            where T : class
        {
            T t = (T)(collectionOrT as T);
            if (t != null)
            {
                return t;
            }
            return ((ReadOnlyCollection<T>)collectionOrT)[0];
        }

        internal static ReadOnlyCollection<Expression> ReturnReadOnly(IArgumentProvider provider, ref object collection)
        {
            Expression expression = collection as Expression;
            if (expression != null)
            {
                System.Threading.Net20Interlocked.CompareExchange(ref collection, new ReadOnlyCollection<Expression>(new ListArgumentProvider(provider, expression)), expression);
            }
            return (ReadOnlyCollection<Expression>)collection;
        }


        [__DynamicallyInvokable]
        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2)
        {
            Expression.RequiresCanRead(arg0, "arg0");
            Expression.RequiresCanRead(arg1, "arg1");
            Expression.RequiresCanRead(arg2, "arg2");
            return new Block3(arg0, arg1, arg2);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3)
        {
            Expression.RequiresCanRead(arg0, "arg0");
            Expression.RequiresCanRead(arg1, "arg1");
            Expression.RequiresCanRead(arg2, "arg2");
            Expression.RequiresCanRead(arg3, "arg3");
            return new Block4(arg0, arg1, arg2, arg3);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Expression arg0, Expression arg1, Expression arg2, Expression arg3, Expression arg4)
        {
            Expression.RequiresCanRead(arg0, "arg0");
            Expression.RequiresCanRead(arg1, "arg1");
            Expression.RequiresCanRead(arg2, "arg2");
            Expression.RequiresCanRead(arg3, "arg3");
            Expression.RequiresCanRead(arg4, "arg4");
            return new Block5(arg0, arg1, arg2, arg3, arg4);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(params Expression[] expressions)
        {
            ContractUtils.RequiresNotNull(expressions, "expressions");
            switch ((int)expressions.Length)
            {
                case 2:
                    {
                        return Expression.Block(expressions[0], expressions[1]);
                    }
                case 3:
                    {
                        return Expression.Block(expressions[0], expressions[1], expressions[2]);
                    }
                case 4:
                    {
                        return Expression.Block(expressions[0], expressions[1], expressions[2], expressions[3]);
                    }
                case 5:
                    {
                        return Expression.Block(expressions[0], expressions[1], expressions[2], expressions[3], expressions[4]);
                    }
            }
            ContractUtils.RequiresNotEmpty<Expression>(expressions, "expressions");
            Expression.RequiresCanRead(expressions, "expressions");
            return new BlockN(expressions.Copy<Expression>());
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(IEnumerable<Expression> expressions)
        {
            return Expression.Block(EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Type type, params Expression[] expressions)
        {
            ContractUtils.RequiresNotNull(expressions, "expressions");
            return Expression.Block(type, (IEnumerable<Expression>)expressions);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Type type, IEnumerable<Expression> expressions)
        {
            return Expression.Block(type, EmptyReadOnlyCollection<ParameterExpression>.Instance, expressions);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions)
        {
            return Expression.Block(variables, (IEnumerable<Expression>)expressions);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, params Expression[] expressions)
        {
            return Expression.Block(type, variables, (IEnumerable<Expression>)expressions);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions)
        {
            ContractUtils.RequiresNotNull(expressions, "expressions");
            ReadOnlyCollection<Expression> readOnly = expressions.ToReadOnly<Expression>();
            ContractUtils.RequiresNotEmpty<Expression>(readOnly, "expressions");
            Expression.RequiresCanRead(readOnly, "expressions");
            return Expression.Block(readOnly.Last<Expression>().Type, variables, readOnly);
        }

        [__DynamicallyInvokable]
        public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions)
        {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(expressions, "expressions");
            ReadOnlyCollection<Expression> readOnly = expressions.ToReadOnly<Expression>();
            ReadOnlyCollection<ParameterExpression> parameterExpressions = variables.ToReadOnly<ParameterExpression>();
            ContractUtils.RequiresNotEmpty<Expression>(readOnly, "expressions");
            Expression.RequiresCanRead(readOnly, "expressions");
            Expression.ValidateVariables(parameterExpressions, "variables");
            Expression expression = readOnly.Last<Expression>();
            if (type != typeof(void) && !TypeUtils.AreReferenceAssignable(type, expression.Type))
            {
                throw Error.ArgumentTypesMustMatch();
            }
            if (!TypeUtils.AreEquivalent(type, expression.Type))
            {
                return new ScopeWithType(parameterExpressions, readOnly, type);
            }
            if (readOnly.Count != 1)
            {
                return new ScopeN(parameterExpressions, readOnly);
            }
            return new Scope1(parameterExpressions, readOnly[0]);
        }

        internal static void ValidateVariables(ReadOnlyCollection<ParameterExpression> varList, string collectionName)
        {
            if (varList.Count == 0)
            {
                return;
            }
            int count = varList.Count;
            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>(count);
            for (int i = 0; i < count; i++)
            {
                ParameterExpression item = varList[i];
                if (item == null)
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]", new object[] { collectionName, parameterExpressions.Count }));
                }
                if (item.IsByRef)
                {
                    throw new ArgumentException($"{item.Name} cannot be by ref");
                }
                if (parameterExpressions.Contains(item))
                {
                    throw new ArgumentException($"DuplicateVariable {item}");
                }
                parameterExpressions.Add(item);
            }
        }


        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents a call to a static (Shared in Visual Basic) method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" /> and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> and <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> that represents a static (Shared in Visual Basic) method to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> property equal to.</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="method" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in <paramref name="arguments" /> does not equal the number of parameters for the method represented by <paramref name="method" />.-or-One or more of the elements of <paramref name="arguments" /> is not assignable to the corresponding parameter for the method represented by <paramref name="method" />.</exception>
        public static MethodCallExpression Call(
          MethodInfo method,
          params Expression[] arguments)
        {
            return Expression.Call((Expression)null, method, (IEnumerable<Expression>)((IEnumerable<Expression>)arguments).ToReadOnlyCollection<Expression>());
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents a call to a method that takes arguments.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" /> and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" />, <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" />, and <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="instance">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> property equal to (pass null for a static (Shared in Visual Basic) method).</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> property equal to.</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="method" /> is null.-or-<paramref name="instance" /> is null and <paramref name="method" /> represents an instance method.-or-<paramref name="arguments" /> is not null and one or more of its elements is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="instance" />.Type is not assignable to the declaring type of the method represented by <paramref name="method" />.-or-The number of elements in <paramref name="arguments" /> does not equal the number of parameters for the method represented by <paramref name="method" />.-or-One or more of the elements of <paramref name="arguments" /> is not assignable to the corresponding parameter for the method represented by <paramref name="method" />.</exception>
        public static MethodCallExpression Call(
          Expression instance,
          MethodInfo method,
          params Expression[] arguments)
        {
            return Expression.Call(instance, method, (IEnumerable<Expression>)((IEnumerable<Expression>)arguments).ToReadOnlyCollection<Expression>());
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents a call to a method that takes arguments.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" /> and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" />, <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" />, and <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="instance">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> property equal to (pass null for a static (Shared in Visual Basic) method).</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> property equal to.</param>
        /// <param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="method" /> is null.-or-<paramref name="instance" /> is null and <paramref name="method" /> represents an instance method.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="instance" />.Type is not assignable to the declaring type of the method represented by <paramref name="method" />.-or-The number of elements in <paramref name="arguments" /> does not equal the number of parameters for the method represented by <paramref name="method" />.-or-One or more of the elements of <paramref name="arguments" /> is not assignable to the corresponding parameter for the method represented by <paramref name="method" />.</exception>
        public static MethodCallExpression Call(
          Expression instance,
          MethodInfo method,
          IEnumerable<Expression> arguments)
        {
            ReadOnlyCollection<Expression> readOnlyCollection = arguments.ToReadOnlyCollection<Expression>();
            Expression.ValidateCallArgs(instance, method, ref readOnlyCollection);
            return new MethodCallExpression(ExpressionType.Call, method, instance, readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents a call to a method that takes no arguments.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" /> and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> and <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="instance">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> property equal to (pass null for a static (Shared in Visual Basic) method).</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="method" /> is null.-or-<paramref name="instance" /> is null and <paramref name="method" /> represents an instance method.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="instance" />.Type is not assignable to the declaring type of the method represented by <paramref name="method" />.</exception>
        public static MethodCallExpression Call(
          Expression instance,
          MethodInfo method)
        {
            return Expression.Call(instance, method, (Expression[])null);
        }

        private static void ValidateCallArgs(
          Expression instance,
          MethodInfo method,
          ref ReadOnlyCollection<Expression> arguments)
        {
            if (method == null)
                throw Error.ArgumentNull(nameof(method));
            if (arguments == null)
                throw Error.ArgumentNull(nameof(arguments));
            Expression.ValidateMethodInfo(method);
            if (!method.IsStatic)
            {
                if (instance == null)
                    throw Error.ArgumentNull(nameof(instance));
                Expression.ValidateCallInstanceType(instance.Type, method);
            }
            Expression.ValidateArgumentTypes(method, ref arguments);
        }

        private static void ValidateCallInstanceType(Type instanceType, MethodInfo method)
        {
            if (!Expression.AreReferenceAssignable(method.DeclaringType, instanceType))
            {
                if (instanceType.IsValueType)
                {
                    if (Expression.AreReferenceAssignable(method.DeclaringType, typeof(object)) || Expression.AreReferenceAssignable(method.DeclaringType, typeof(ValueType)) || instanceType.IsEnum && Expression.AreReferenceAssignable(method.DeclaringType, typeof(Enum)))
                        return;
                    if (method.DeclaringType.IsInterface)
                    {
                        foreach (Type src in instanceType.GetInterfaces())
                        {
                            if (Expression.AreReferenceAssignable(method.DeclaringType, src))
                                return;
                        }
                    }
                }
                throw Error.MethodNotDefinedForType((object)method, (object)instanceType);
            }
        }

        private static void ValidateArgumentTypes(
          MethodInfo method,
          ref ReadOnlyCollection<Expression> arguments)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                if (parameters.Length != arguments.Count)
                    throw Error.IncorrectNumberOfMethodCallArguments((object)method);
                List<Expression> sequence = (List<Expression>)null;
                int index1 = 0;
                for (int length = parameters.Length; index1 < length; ++index1)
                {
                    Expression expression = arguments[index1];
                    ParameterInfo parameterInfo = parameters[index1];
                    if (expression == null)
                        throw Error.ArgumentNull(nameof(arguments));
                    Type type = parameterInfo.ParameterType;
                    if (type.IsByRef)
                        type = type.GetElementType();
                    Expression.ValidateType(type);
                    if (!Expression.AreReferenceAssignable(type, expression.Type))
                        expression = Expression.IsSameOrSubclass(typeof(Expression), type) && Expression.AreAssignable(type, expression.GetType()) ? (Expression)Expression.Quote(expression) : throw Error.ExpressionTypeDoesNotMatchMethodParameter((object)expression.Type, (object)type, (object)method);
                    if (sequence == null && expression != arguments[index1])
                    {
                        sequence = new List<Expression>(arguments.Count);
                        for (int index2 = 0; index2 < index1; ++index2)
                            sequence.Add(arguments[index2]);
                    }
                    sequence?.Add(expression);
                }
                if (sequence == null)
                    return;
                arguments = sequence.ToReadOnlyCollection<Expression>();
            }
            else if (arguments.Count > 0)
                throw Error.IncorrectNumberOfMethodCallArguments((object)method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents a call to an instance method by calling the appropriate factory method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" />, the <see cref="P:System.Linq.Expressions.MethodCallExpression.Object" /> property equal to <paramref name="instance" />, <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> set to the <see cref="T:System.Reflection.MethodInfo" /> that represents the specified instance method, and <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> set to the specified arguments.</returns>
        /// <param name="instance">An <see cref="T:System.Linq.Expressions.Expression" /> whose <see cref="P:System.Linq.Expressions.Expression.Type" /> property value will be searched for a specific method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="typeArguments">An array of <see cref="T:System.Type" /> objects that specify the type parameters of the method.</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects that represents the arguments to the method.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="instance" /> or <paramref name="methodName" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">No method whose name is <paramref name="methodName" />, whose type parameters match <paramref name="typeArguments" />, and whose parameter types match <paramref name="arguments" /> is found in <paramref name="instance" />.Type or its base types.-or-More than one method whose name is <paramref name="methodName" />, whose type parameters match <paramref name="typeArguments" />, and whose parameter types match <paramref name="arguments" /> is found in <paramref name="instance" />.Type or its base types.</exception>
        public static MethodCallExpression Call(
          Expression instance,
          string methodName,
          Type[] typeArguments,
          params Expression[] arguments)
        {
            if (instance == null)
                throw Error.ArgumentNull(nameof(instance));
            if (methodName == null)
                throw Error.ArgumentNull(nameof(methodName));
            if (arguments == null)
                arguments = new Expression[0];
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return Expression.Call(instance, Expression.FindMethod(instance.Type, methodName, typeArguments, arguments, flags), arguments);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that represents a call to a static (Shared in Visual Basic) method by calling the appropriate factory method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MethodCallExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Call" />, the <see cref="P:System.Linq.Expressions.MethodCallExpression.Method" /> property set to the <see cref="T:System.Reflection.MethodInfo" /> that represents the specified static (Shared in Visual Basic) method, and the <see cref="P:System.Linq.Expressions.MethodCallExpression.Arguments" /> property set to the specified arguments.</returns>
        /// <param name="type">The <see cref="T:System.Type" /> that specifies the type that contains the specified static (Shared in Visual Basic) method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="typeArguments">An array of <see cref="T:System.Type" /> objects that specify the type parameters of the method.</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects that represent the arguments to the method.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="methodName" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">No method whose name is <paramref name="methodName" />, whose type parameters match <paramref name="typeArguments" />, and whose parameter types match <paramref name="arguments" /> is found in <paramref name="type" /> or its base types.-or-More than one method whose name is <paramref name="methodName" />, whose type parameters match <paramref name="typeArguments" />, and whose parameter types match <paramref name="arguments" /> is found in <paramref name="type" /> or its base types.</exception>
        public static MethodCallExpression Call(
          Type type,
          string methodName,
          Type[] typeArguments,
          params Expression[] arguments)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (methodName == null)
                throw Error.ArgumentNull(nameof(methodName));
            if (arguments == null)
                arguments = new Expression[0];
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return Expression.Call((Expression)null, Expression.FindMethod(type, methodName, typeArguments, arguments, flags), arguments);
        }

        private static MethodInfo FindMethod(
          Type type,
          string methodName,
          Type[] typeArgs,
          Expression[] args,
          BindingFlags flags)
        {
            MemberInfo[] members = type.FindMembers(MemberTypes.Method, flags, Type.FilterNameIgnoreCase, (object)methodName);
            if (members == null || members.Length == 0)
                throw Error.MethodDoesNotExistOnType((object)methodName, (object)type);
            MethodInfo method;
            int bestMethod = Expression.FindBestMethod(members.Cast<MethodInfo>(), typeArgs, args, out method);
            if (bestMethod == 0)
                throw Error.MethodWithArgsDoesNotExistOnType((object)methodName, (object)type);
            if (bestMethod > 1)
                throw Error.MethodWithMoreThanOneMatch((object)methodName, (object)type);
            return method;
        }

        private static int FindBestMethod(
          IEnumerable<MethodInfo> methods,
          Type[] typeArgs,
          Expression[] args,
          out MethodInfo method)
        {
            int bestMethod = 0;
            method = (MethodInfo)null;
            foreach (MethodInfo method1 in methods)
            {
                MethodInfo m = Expression.ApplyTypeArgs(method1, typeArgs);
                if (m != null && Expression.IsCompatible(m, args))
                {
                    if (method == null || !method.IsPublic && m.IsPublic)
                    {
                        method = m;
                        bestMethod = 1;
                    }
                    else if (method.IsPublic == m.IsPublic)
                        ++bestMethod;
                }
            }
            return bestMethod;
        }

        private static MethodInfo ApplyTypeArgs(MethodInfo m, Type[] typeArgs)
        {
            if (typeArgs == null || typeArgs.Length == 0)
            {
                if (!m.IsGenericMethodDefinition)
                    return m;
            }
            else if (m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
                return m.MakeGenericMethod(typeArgs);
            return (MethodInfo)null;
        }

        private static bool IsCompatible(MethodInfo m, Expression[] args)
        {
            ParameterInfo[] parameters = m.GetParameters();
            if (parameters.Length != args.Length)
                return false;
            for (int index = 0; index < args.Length; ++index)
            {
                Expression expression = args[index];
                Type src = expression != null ? expression.Type : throw Error.ArgumentNull("argument");
                Type type = parameters[index].ParameterType;
                if (type.IsByRef)
                    type = type.GetElementType();
                if (!Expression.AreReferenceAssignable(type, src) && (!Expression.IsSameOrSubclass(typeof(Expression), type) || !Expression.AreAssignable(type, expression.GetType())))
                    return false;
            }
            return true;
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a coalescing operation, given a conversion function.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Coalesce" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Conversion" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="conversion">A <see cref="T:System.Linq.Expressions.LambdaExpression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Conversion" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="left" />.Type and <paramref name="right" />.Type are not convertible to each other.-or-<paramref name="conversion" /> is not null and <paramref name="conversion" />.Type is a delegate type that does not take exactly one argument.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of <paramref name="left" /> does not represent a reference type or a nullable value type.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of <paramref name="left" /> represents a type that is not assignable to the parameter type of the delegate type <paramref name="conversion" />.Type.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of <paramref name="right" /> is not equal to the return type of the delegate type <paramref name="conversion" />.Type.</exception>
        public static BinaryExpression Coalesce(
          Expression left,
          Expression right,
          LambdaExpression conversion)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            if (conversion == null)
                return Expression.Coalesce(left, right);
            if (left.Type.IsValueType && !Expression.IsNullableType(left.Type))
                throw Error.CoalesceUsedOnNonNullType();
            MethodInfo method = conversion.Type.GetMethod("Invoke");
            ParameterInfo[] parameterInfoArray = method.ReturnType != typeof(void) ? method.GetParameters() : throw Error.UserDefinedOperatorMustNotBeVoid((object)conversion);
            if (parameterInfoArray.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments((object)conversion);
            if (method.ReturnType != right.Type)
                throw Error.OperandTypesDoNotMatchParameters((object)ExpressionType.Coalesce, (object)conversion.ToString());
            if (!Expression.ParameterIsAssignable(parameterInfoArray[0], Expression.GetNonNullableType(left.Type)) && !Expression.ParameterIsAssignable(parameterInfoArray[0], left.Type))
                throw Error.OperandTypesDoNotMatchParameters((object)ExpressionType.Coalesce, (object)conversion.ToString());
            return new BinaryExpression(ExpressionType.Coalesce, left, right, conversion, right.Type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a coalescing operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Coalesce" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of <paramref name="left" /> does not represent a reference type or a nullable value type.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="left" />.Type and <paramref name="right" />.Type are not convertible to each other.</exception>
        public static BinaryExpression Coalesce(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            Type type = Expression.ValidateCoalesceArgTypes(left.Type, right.Type);
            return new BinaryExpression(ExpressionType.Coalesce, left, right, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ConditionalExpression" />.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ConditionalExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Conditional" /> and the <see cref="P:System.Linq.Expressions.ConditionalExpression.Test" />, <see cref="P:System.Linq.Expressions.ConditionalExpression.IfTrue" />, and <see cref="P:System.Linq.Expressions.ConditionalExpression.IfFalse" /> properties set to the specified values.</returns>
        /// <param name="test">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.ConditionalExpression.Test" /> property equal to.</param>
        /// <param name="ifTrue">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.ConditionalExpression.IfTrue" /> property equal to.</param>
        /// <param name="ifFalse">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.ConditionalExpression.IfFalse" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="test" /> or <paramref name="ifTrue" /> or <paramref name="ifFalse" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="test" />.Type is not <see cref="T:System.Boolean" />.-or-<paramref name="ifTrue" />.Type is not equal to <paramref name="ifFalse" />.Type.</exception>
        public static ConditionalExpression Condition(
          Expression test,
          Expression ifTrue,
          Expression ifFalse)
        {
            if (test == null)
                throw Error.ArgumentNull(nameof(test));
            if (ifTrue == null)
                throw Error.ArgumentNull(nameof(ifTrue));
            if (ifFalse == null)
                throw Error.ArgumentNull(nameof(ifFalse));
            if (test.Type != typeof(bool))
                throw Error.ArgumentMustBeBoolean();
            Expression.ValidateSameArgTypes(ifTrue.Type, ifFalse.Type);
            return new ConditionalExpression(test, ifTrue, ifFalse, ifTrue.Type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ConstantExpression" /> that has the <see cref="P:System.Linq.Expressions.ConstantExpression.Value" /> property set to the specified value.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ConstantExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Constant" /> and the <see cref="P:System.Linq.Expressions.ConstantExpression.Value" /> property set to the specified value.</returns>
        /// <param name="value">An <see cref="T:System.Object" /> to set the <see cref="P:System.Linq.Expressions.ConstantExpression.Value" /> property equal to.</param>
        public static ConstantExpression Constant(object value)
        {
            Type type = value != null ? value.GetType() : typeof(object);
            return Expression.Constant(value, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ConstantExpression" /> that has the <see cref="P:System.Linq.Expressions.ConstantExpression.Value" /> and <see cref="P:System.Linq.Expressions.Expression.Type" /> properties set to the specified values.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ConstantExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Constant" /> and the <see cref="P:System.Linq.Expressions.ConstantExpression.Value" /> and <see cref="P:System.Linq.Expressions.Expression.Type" /> properties set to the specified values.</returns>
        /// <param name="value">An <see cref="T:System.Object" /> to set the <see cref="P:System.Linq.Expressions.ConstantExpression.Value" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="value" /> is not null and <paramref name="type" /> is not assignable from the dynamic type of <paramref name="value" />.</exception>
        public static ConstantExpression Constant(object value, Type type)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (value == null && type.IsValueType && !Expression.IsNullableType(type))
                throw Error.ArgumentTypesMustMatch();
            return value == null || Expression.AreAssignable(type, value.GetType()) ? new ConstantExpression(value, type) : throw Error.ArgumentTypesMustMatch();
        }

        private static bool HasIdentityPrimitiveOrNullableConversion(Type source, Type dest) => source == dest || Expression.IsNullableType(source) && dest == Expression.GetNonNullableType(source) || Expression.IsNullableType(dest) && source == Expression.GetNonNullableType(dest) || Expression.IsConvertible(source) && Expression.IsConvertible(dest) && Expression.GetNonNullableType(dest) != typeof(bool);

        private static bool HasReferenceConversion(Type source, Type dest)
        {
            Type nonNullableType1 = Expression.GetNonNullableType(source);
            Type nonNullableType2 = Expression.GetNonNullableType(dest);
            return Expression.AreAssignable(nonNullableType1, nonNullableType2) || Expression.AreAssignable(nonNullableType2, nonNullableType1) || source.IsInterface || dest.IsInterface || source == typeof(object) || dest == typeof(object);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a conversion operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Convert" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.Expression.Type" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="type" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">No conversion operator is defined between <paramref name="expression" />.Type and <paramref name="type" />.</exception>
        public static UnaryExpression Convert(Expression expression, Type type)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            return Expression.HasIdentityPrimitiveOrNullableConversion(expression.Type, type) || Expression.HasReferenceConversion(expression.Type, type) ? new UnaryExpression(ExpressionType.Convert, expression, type) : Expression.GetUserDefinedCoercionOrThrow(ExpressionType.Convert, expression, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a conversion operation for which the implementing method is specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Convert" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" />, <see cref="P:System.Linq.Expressions.Expression.Type" />, and <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="type" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly one argument.</exception>
        /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method that matches the <paramref name="method" /> description was found.</exception>
        /// <exception cref="T:System.InvalidOperationException">No conversion operator is defined between <paramref name="expression" />.Type and <paramref name="type" />.-or-<paramref name="expression" />.Type is not assignable to the argument type of the method represented by <paramref name="method" />.-or-The return type of the method represented by <paramref name="method" /> is not assignable to <paramref name="type" />.-or-<paramref name="expression" />.Type or <paramref name="type" /> is a nullable value type and the corresponding non-nullable value type does not equal the argument type or the return type, respectively, of the method represented by <paramref name="method" />.</exception>
        public static UnaryExpression Convert(
          Expression expression,
          Type type,
          MethodInfo method)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return method == null ? Expression.Convert(expression, type) : Expression.GetMethodBasedCoercionOperator(ExpressionType.Convert, expression, type, method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a conversion operation that throws an exception if the target type is overflowed.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ConvertChecked" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.Expression.Type" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="type" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">No conversion operator is defined between <paramref name="expression" />.Type and <paramref name="type" />.</exception>
        public static UnaryExpression ConvertChecked(Expression expression, Type type)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (Expression.HasIdentityPrimitiveOrNullableConversion(expression.Type, type))
                return new UnaryExpression(ExpressionType.ConvertChecked, expression, type);
            return Expression.HasReferenceConversion(expression.Type, type) ? new UnaryExpression(ExpressionType.Convert, expression, type) : Expression.GetUserDefinedCoercionOrThrow(ExpressionType.ConvertChecked, expression, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a conversion operation that throws an exception if the target type is overflowed and for which the implementing method is specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ConvertChecked" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" />, <see cref="P:System.Linq.Expressions.Expression.Type" />, and <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="type" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly one argument.</exception>
        /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one method that matches the <paramref name="method" /> description was found.</exception>
        /// <exception cref="T:System.InvalidOperationException">No conversion operator is defined between <paramref name="expression" />.Type and <paramref name="type" />.-or-<paramref name="expression" />.Type is not assignable to the argument type of the method represented by <paramref name="method" />.-or-The return type of the method represented by <paramref name="method" /> is not assignable to <paramref name="type" />.-or-<paramref name="expression" />.Type or <paramref name="type" /> is a nullable value type and the corresponding non-nullable value type does not equal the argument type or the return type, respectively, of the method represented by <paramref name="method" />.</exception>
        public static UnaryExpression ConvertChecked(
          Expression expression,
          Type type,
          MethodInfo method)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return method == null ? Expression.ConvertChecked(expression, type) : Expression.GetMethodBasedCoercionOperator(ExpressionType.ConvertChecked, expression, type, method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic division operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Divide" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The division operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Divide(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.Divide, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Divide, "op_Division", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic division operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Divide" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the division operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Divide(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Divide(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Divide, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an equality comparison.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Equal" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The equality operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Equal(Expression left, Expression right) => Expression.Equal(left, right, false, (MethodInfo)null);

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an equality comparison. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Equal" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the equality operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Equal(
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.GetEqualityComparisonOperator(ExpressionType.Equal, "op_Equality", left, right, liftToNull) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Equal, left, right, method, liftToNull);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise XOR operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ExclusiveOr" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The XOR operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression ExclusiveOr(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsIntegerOrBool(left.Type) ? new BinaryExpression(ExpressionType.ExclusiveOr, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.ExclusiveOr, "op_ExclusiveOr", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise XOR operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ExclusiveOr" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the XOR operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression ExclusiveOr(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.ExclusiveOr(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.ExclusiveOr, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing a field.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberAccess" /> and the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> and <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property equal to.</param>
        /// <param name="field">The <see cref="T:System.Reflection.FieldInfo" /> to set the <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="field" /> is null.-or-The field represented by <paramref name="field" /> is not static (Shared in Visual Basic) and <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="expression" />.Type is not assignable to the declaring type of the field represented by <paramref name="field" />.</exception>
        public static MemberExpression Field(Expression expression, FieldInfo field)
        {
            if (field == null)
                throw Error.ArgumentNull(nameof(field));
            if (!field.IsStatic)
            {
                if (expression == null)
                    throw Error.ArgumentNull(nameof(expression));
                if (!Expression.AreReferenceAssignable(field.DeclaringType, expression.Type))
                    throw Error.FieldNotDefinedForType((object)field, (object)expression.Type);
            }
            return new MemberExpression(expression, (MemberInfo)field, field.FieldType);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing a field given the name of the field.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberAccess" />, the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property set to <paramref name="expression" />, and the <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> property set to the <see cref="T:System.Reflection.FieldInfo" /> that represents the field denoted by <paramref name="fieldName" />.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> whose <see cref="P:System.Linq.Expressions.Expression.Type" /> contains a field named <paramref name="fieldName" />.</param>
        /// <param name="fieldName">The name of a field.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="fieldName" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">No field named <paramref name="fieldName" /> is defined in <paramref name="expression" />.Type or its base types.</exception>
        public static MemberExpression Field(Expression expression, string fieldName)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return Expression.Field(expression, (expression.Type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy) ?? expression.Type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) ?? throw Error.FieldNotDefinedForType((object)fieldName, (object)expression.Type));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a "greater than or equal" numeric comparison.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.GreaterThanOrEqual" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The "greater than or equal" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression GreaterThanOrEqual(
          Expression left,
          Expression right)
        {
            return Expression.GreaterThanOrEqual(left, right, false, (MethodInfo)null);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a "greater than or equal" numeric comparison. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.GreaterThanOrEqual" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the "greater than or equal" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression GreaterThanOrEqual(
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.GetComparisonOperator(ExpressionType.GreaterThanOrEqual, "op_GreaterThanOrEqual", left, right, liftToNull) : Expression.GetMethodBasedBinaryOperator(ExpressionType.GreaterThanOrEqual, left, right, method, liftToNull);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a "greater than" numeric comparison.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.GreaterThan" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The "greater than" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression GreaterThan(Expression left, Expression right) => Expression.GreaterThan(left, right, false, (MethodInfo)null);

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a "greater than" numeric comparison. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.GreaterThan" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the "greater than" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression GreaterThan(
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.GetComparisonOperator(ExpressionType.GreaterThan, "op_GreaterThan", left, right, liftToNull) : Expression.GetMethodBasedBinaryOperator(ExpressionType.GreaterThan, left, right, method, liftToNull);
        }

        /// <summary>Creates an <see cref="T:System.Linq.Expressions.InvocationExpression" />.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.InvocationExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Invoke" /> and the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> and <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> equal to</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="expression" />.Type does not represent a delegate type or an <see cref="T:System.Linq.Expressions.Expression`1" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the delegate represented by <paramref name="expression" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="arguments" /> does not contain the same number of elements as the list of parameters for the delegate represented by <paramref name="expression" />.</exception>
        public static InvocationExpression Invoke(
          Expression expression,
          params Expression[] arguments)
        {
            return Expression.Invoke(expression, (IEnumerable<Expression>)((IEnumerable<Expression>)arguments).ToReadOnlyCollection<Expression>());
        }

        /// <summary>Creates an <see cref="T:System.Linq.Expressions.InvocationExpression" />.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.InvocationExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Invoke" /> and the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> and <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> equal to</param>
        /// <param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="expression" />.Type does not represent a delegate type or an <see cref="T:System.Linq.Expressions.Expression`1" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the delegate represented by <paramref name="expression" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="arguments" /> does not contain the same number of elements as the list of parameters for the delegate represented by <paramref name="expression" />.</exception>
        public static InvocationExpression Invoke(
          Expression expression,
          IEnumerable<Expression> arguments)
        {
            Type p0 = expression != null ? expression.Type : throw Error.ArgumentNull(nameof(expression));
            if (p0 == typeof(Delegate))
                throw Error.ExpressionTypeNotInvocable((object)p0);
            if (!Expression.AreAssignable(typeof(Delegate), expression.Type))
                p0 = (TypeHelper.FindGenericType(typeof(Expression<>), expression.Type) ?? throw Error.ExpressionTypeNotInvocable((object)expression.Type)).GetGenericArguments()[0];
            MethodInfo method = p0.GetMethod(nameof(Invoke));
            ParameterInfo[] parameters = method.GetParameters();
            ReadOnlyCollection<Expression> readOnlyCollection = arguments.ToReadOnlyCollection<Expression>();
            if (parameters.Length > 0)
            {
                if (readOnlyCollection.Count != parameters.Length)
                    throw Error.IncorrectNumberOfLambdaArguments();
                List<Expression> sequence = (List<Expression>)null;
                int index1 = 0;
                for (int count = readOnlyCollection.Count; index1 < count; ++index1)
                {
                    Expression expression1 = readOnlyCollection[index1];
                    ParameterInfo parameterInfo = parameters[index1];
                    if (expression1 == null)
                        throw Error.ArgumentNull(nameof(arguments));
                    Type type = parameterInfo.ParameterType;
                    if (type.IsByRef)
                        type = type.GetElementType();
                    if (!Expression.AreReferenceAssignable(type, expression1.Type))
                        expression1 = Expression.IsSameOrSubclass(typeof(Expression), type) && Expression.AreAssignable(type, expression1.GetType()) ? (Expression)Expression.Quote(expression1) : throw Error.ExpressionTypeDoesNotMatchParameter((object)expression1.Type, (object)type);
                    if (sequence == null && expression1 != readOnlyCollection[index1])
                    {
                        sequence = new List<Expression>(readOnlyCollection.Count);
                        for (int index2 = 0; index2 < index1; ++index2)
                            sequence.Add(readOnlyCollection[index2]);
                    }
                    sequence?.Add(expression1);
                }
                if (sequence != null)
                    readOnlyCollection = sequence.ToReadOnlyCollection<Expression>();
            }
            else if (readOnlyCollection.Count > 0)
                throw Error.IncorrectNumberOfLambdaArguments();
            return new InvocationExpression(expression, method.ReturnType, readOnlyCollection);
        }

        /// <summary>Creates an <see cref="T:System.Linq.Expressions.Expression`1" /> where the delegate type is known at compile time.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression`1" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Lambda" /> and the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> and <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> properties set to the specified values.</returns>
        /// <param name="body">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> property equal to.</param>
        /// <param name="parameters">An array of <see cref="T:System.Linq.Expressions.ParameterExpression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> collection.</param>
        /// <typeparam name="TDelegate">A delegate type.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="body" /> is null.-or-One or more elements in <paramref name="parameters" /> are null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="TDelegate" /> is not a delegate type.-or-<paramref name="body" />.Type represents a type that is not assignable to the return type of <paramref name="TDelegate" />.-or-<paramref name="parameters" /> does not contain the same number of elements as the list of parameters for <paramref name="TDelegate" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="parameters" /> is not assignable from the type of the corresponding parameter type of <paramref name="TDelegate" />.</exception>
        public static Expression<TDelegate> Lambda<TDelegate>(
          Expression body,
          params ParameterExpression[] parameters)
        {
            return Expression.Lambda<TDelegate>(body, (IEnumerable<ParameterExpression>)((IEnumerable<ParameterExpression>)parameters).ToReadOnlyCollection<ParameterExpression>());
        }

        /// <summary>Creates an <see cref="T:System.Linq.Expressions.Expression`1" /> where the delegate type is known at compile time.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression`1" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Lambda" /> and the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> and <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> properties set to the specified values.</returns>
        /// <param name="body">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> property equal to.</param>
        /// <param name="parameters">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.ParameterExpression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> collection.</param>
        /// <typeparam name="TDelegate">A delegate type.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="body" /> is null.-or-One or more elements in <paramref name="parameters" /> are null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="TDelegate" /> is not a delegate type.-or-<paramref name="body" />.Type represents a type that is not assignable to the return type of <paramref name="TDelegate" />.-or-<paramref name="parameters" /> does not contain the same number of elements as the list of parameters for <paramref name="TDelegate" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="parameters" /> is not assignable from the type of the corresponding parameter type of <paramref name="TDelegate" />.</exception>
        public static Expression<TDelegate> Lambda<TDelegate>(
          Expression body,
          IEnumerable<ParameterExpression> parameters)
        {
            if (body == null)
                throw Error.ArgumentNull(nameof(body));
            ReadOnlyCollection<ParameterExpression> readOnlyCollection = parameters.ToReadOnlyCollection<ParameterExpression>();
            Expression.ValidateLambdaArgs(typeof(TDelegate), ref body, readOnlyCollection);
            return new Expression<TDelegate>(body, readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.LambdaExpression" /> and can be used when the delegate type is not known at compile time.</summary>
        /// <returns>An object that represents a lambda expression which has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Lambda" /> and the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> and <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> properties set to the specified values.</returns>
        /// <param name="delegateType">A <see cref="T:System.Type" /> that represents a delegate type.</param>
        /// <param name="body">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> property equal to.</param>
        /// <param name="parameters">An array of <see cref="T:System.Linq.Expressions.ParameterExpression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="delegateType" /> or <paramref name="body" /> is null.-or-One or more elements in <paramref name="parameters" /> are null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="delegateType" /> does not represent a delegate type.-or-<paramref name="body" />.Type represents a type that is not assignable to the return type of the delegate type represented by <paramref name="delegateType" />.-or-<paramref name="parameters" /> does not contain the same number of elements as the list of parameters for the delegate type represented by <paramref name="delegateType" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="parameters" /> is not assignable from the type of the corresponding parameter type of the delegate type represented by <paramref name="delegateType" />.</exception>
        public static LambdaExpression Lambda(
          Type delegateType,
          Expression body,
          params ParameterExpression[] parameters)
        {
            return Expression.Lambda(delegateType, body, (IEnumerable<ParameterExpression>)((IEnumerable<ParameterExpression>)parameters).ToReadOnlyCollection<ParameterExpression>());
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.LambdaExpression" /> and can be used when the delegate type is not known at compile time.</summary>
        /// <returns>An object that represents a lambda expression which has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Lambda" /> and the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> and <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> properties set to the specified values.</returns>
        /// <param name="delegateType">A <see cref="T:System.Type" /> that represents a delegate type.</param>
        /// <param name="body">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> property equal to.</param>
        /// <param name="parameters">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.ParameterExpression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="delegateType" /> or <paramref name="body" /> is null.-or-One or more elements in <paramref name="parameters" /> are null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="delegateType" /> does not represent a delegate type.-or-<paramref name="body" />.Type represents a type that is not assignable to the return type of the delegate type represented by <paramref name="delegateType" />.-or-<paramref name="parameters" /> does not contain the same number of elements as the list of parameters for the delegate type represented by <paramref name="delegateType" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="parameters" /> is not assignable from the type of the corresponding parameter type of the delegate type represented by <paramref name="delegateType" />.</exception>
        public static LambdaExpression Lambda(
          Type delegateType,
          Expression body,
          IEnumerable<ParameterExpression> parameters)
        {
            if (delegateType == null)
                throw Error.ArgumentNull(nameof(delegateType));
            if (body == null)
                throw Error.ArgumentNull(nameof(body));
            ReadOnlyCollection<ParameterExpression> readOnlyCollection = parameters.ToReadOnlyCollection<ParameterExpression>();
            Expression.ValidateLambdaArgs(delegateType, ref body, readOnlyCollection);
            return (LambdaExpression)typeof(Expression).GetMethod(nameof(Lambda), BindingFlags.Static | BindingFlags.Public, (Binder)null, Expression.lambdaTypes, (ParameterModifier[])null).MakeGenericMethod(delegateType).Invoke((object)null, new object[2]
            {
        (object) body,
        (object) readOnlyCollection
            });
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.LambdaExpression" /> by first constructing a delegate type.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.LambdaExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Lambda" /> and the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> and <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> properties set to the specified values.</returns>
        /// <param name="body">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.LambdaExpression.Body" /> property equal to.</param>
        /// <param name="parameters">An array of <see cref="T:System.Linq.Expressions.ParameterExpression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.LambdaExpression.Parameters" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="body" /> is null.-or-One or more elements of <paramref name="parameters" /> are null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="parameters" /> contains more than four elements.</exception>
        public static LambdaExpression Lambda(
          Expression body,
          params ParameterExpression[] parameters)
        {
            if (body == null)
                throw Error.ArgumentNull(nameof(body));
            bool flag = body.Type == typeof(void);
            int index1 = parameters == null ? 0 : parameters.Length;
            Type[] typeArray = new Type[index1 + (flag ? 0 : 1)];
            for (int index2 = 0; index2 < index1; ++index2)
            {
                if (parameters[index2] == null)
                    throw Error.ArgumentNull("parameter");
                typeArray[index2] = parameters[index2].Type;
            }
            Type delegateType;
            if (flag)
            {
                delegateType = Expression.GetActionType(typeArray);
            }
            else
            {
                typeArray[index1] = body.Type;
                delegateType = Expression.GetFuncType(typeArray);
            }
            return Expression.Lambda(delegateType, body, parameters);
        }

        /// <summary>Creates a <see cref="T:System.Type" /> object that represents a generic System.Func delegate type that has specific type arguments.</summary>
        /// <returns>The type of a System.Func delegate that has the specified type arguments.</returns>
        /// <param name="typeArgs">An array of one to five <see cref="T:System.Type" /> objects that specify the type arguments for the System.Func delegate type.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="typeArgs" /> contains less than one or more than five elements.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="typeArgs" /> is null.</exception>
        public static Type GetFuncType(params Type[] typeArgs)
        {
            if (typeArgs == null)
                throw Error.ArgumentNull(nameof(typeArgs));
            if (typeArgs.Length < 1 || typeArgs.Length > 5)
                throw Error.IncorrectNumberOfTypeArgsForFunc();
            return Expression.funcTypes[typeArgs.Length - 1].MakeGenericType(typeArgs);
        }

        /// <summary>Creates a <see cref="T:System.Type" /> object that represents a generic System.Action delegate type that has specific type arguments.</summary>
        /// <returns>The type of a System.Action delegate that has the specified type arguments.</returns>
        /// <param name="typeArgs">An array of zero to four <see cref="T:System.Type" /> objects that specify the type arguments for the System.Action delegate type.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="typeArgs" /> contains more than four elements.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="typeArgs" /> is null.</exception>
        public static Type GetActionType(params Type[] typeArgs)
        {
            if (typeArgs == null)
                throw Error.ArgumentNull(nameof(typeArgs));
            if (typeArgs.Length >= Expression.actionTypes.Length)
                throw Error.IncorrectNumberOfTypeArgsForAction();
            return typeArgs.Length == 0 ? Expression.actionTypes[typeArgs.Length] : Expression.actionTypes[typeArgs.Length].MakeGenericType(typeArgs);
        }

        private static void ValidateLambdaArgs(
          Type delegateType,
          ref Expression body,
          ReadOnlyCollection<ParameterExpression> parameters)
        {
            if (delegateType == null)
                throw Error.ArgumentNull(nameof(delegateType));
            if (body == null)
                throw Error.ArgumentNull(nameof(body));
            MethodInfo methodInfo = Expression.AreAssignable(typeof(Delegate), delegateType) && delegateType != typeof(Delegate) ? delegateType.GetMethod("Invoke") : throw Error.LambdaTypeMustBeDerivedFromSystemDelegate();
            ParameterInfo[] parameters1 = methodInfo.GetParameters();
            if (parameters1.Length > 0)
            {
                if (parameters1.Length != parameters.Count)
                    throw Error.IncorrectNumberOfLambdaDeclarationParameters();
                int index = 0;
                for (int length = parameters1.Length; index < length; ++index)
                {
                    Expression parameter = (Expression)parameters[index];
                    ParameterInfo parameterInfo = parameters1[index];
                    if (parameter == null)
                        throw Error.ArgumentNull(nameof(parameters));
                    Type parameterType = parameterInfo.ParameterType;
                    if (parameterType.IsByRef || parameter.Type.IsByRef)
                        throw Error.ExpressionMayNotContainByrefParameters();
                    if (!Expression.AreReferenceAssignable(parameter.Type, parameterType))
                        throw Error.ParameterExpressionNotValidAsDelegate((object)parameter.Type, (object)parameterType);
                }
            }
            else if (parameters.Count > 0)
                throw Error.IncorrectNumberOfLambdaDeclarationParameters();
            if (methodInfo.ReturnType == typeof(void) || Expression.AreReferenceAssignable(methodInfo.ReturnType, body.Type))
                return;
            if (!Expression.IsSameOrSubclass(typeof(Expression), methodInfo.ReturnType) || !Expression.AreAssignable(methodInfo.ReturnType, body.GetType()))
                throw Error.ExpressionTypeDoesNotMatchReturn((object)body.Type, (object)methodInfo.ReturnType);
            body = (Expression)Expression.Quote(body);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise left-shift operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.LeftShift" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The left-shift operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression LeftShift(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return Expression.IsInteger(left.Type) && Expression.GetNonNullableType(right.Type) == typeof(int) ? new BinaryExpression(ExpressionType.LeftShift, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.LeftShift, "op_LeftShift", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise left-shift operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.LeftShift" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the left-shift operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression LeftShift(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.LeftShift(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.LeftShift, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a "less than" numeric comparison.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.LessThan" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The "less than" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression LessThan(Expression left, Expression right) => Expression.LessThan(left, right, false, (MethodInfo)null);

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a "less than" numeric comparison. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.LessThan" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the "less than" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression LessThan(
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.GetComparisonOperator(ExpressionType.LessThan, "op_LessThan", left, right, liftToNull) : Expression.GetMethodBasedBinaryOperator(ExpressionType.LessThan, left, right, method, liftToNull);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a " less than or equal" numeric comparison.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.LessThanOrEqual" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The "less than or equal" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression LessThanOrEqual(
          Expression left,
          Expression right)
        {
            return Expression.LessThanOrEqual(left, right, false, (MethodInfo)null);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a " less than or equal" numeric comparison. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.LessThanOrEqual" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the "less than or equal" operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression LessThanOrEqual(
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.GetComparisonOperator(ExpressionType.LessThanOrEqual, "op_LessThanOrEqual", left, right, liftToNull) : Expression.GetMethodBasedBinaryOperator(ExpressionType.LessThanOrEqual, left, right, method, liftToNull);
        }

        internal static void ValidateLift(
          IEnumerable<ParameterExpression> parameters,
          IEnumerable<Expression> arguments)
        {
            ReadOnlyCollection<ParameterExpression> readOnlyCollection1 = parameters.ToReadOnlyCollection<ParameterExpression>();
            ReadOnlyCollection<Expression> readOnlyCollection2 = arguments.ToReadOnlyCollection<Expression>();
            if (readOnlyCollection1.Count != readOnlyCollection2.Count)
                throw Error.IncorrectNumberOfIndexes();
            int index = 0;
            for (int count = readOnlyCollection1.Count; index < count; ++index)
            {
                if (!Expression.AreReferenceAssignable(readOnlyCollection1[index].Type, Expression.GetNonNullableType(readOnlyCollection2[index].Type)))
                    throw Error.ArgumentTypesMustMatch();
            }
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ListInitExpression" /> that uses a method named "Add" to add elements to a collection.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ListInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ListInit" /> and the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property set to the specified value.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="initializers">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="initializers" /> is null.-or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="newExpression" />.Type does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">There is no instance method named "Add" (case insensitive) declared in <paramref name="newExpression" />.Type or its base type.-or-The add method on <paramref name="newExpression" />.Type or its base type does not take exactly one argument.-or-The type represented by the <see cref="P:System.Linq.Expressions.Expression.Type" /> property of the first element of <paramref name="initializers" /> is not assignable to the argument type of the add method on <paramref name="newExpression" />.Type or its base type.-or-More than one argument-compatible method named "Add" (case-insensitive) exists on <paramref name="newExpression" />.Type and/or its base type.</exception>
        public static ListInitExpression ListInit(
          NewExpression newExpression,
          params Expression[] initializers)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            return initializers != null ? Expression.ListInit(newExpression, (IEnumerable<Expression>)initializers) : throw Error.ArgumentNull(nameof(initializers));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ListInitExpression" /> that uses a method named "Add" to add elements to a collection.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ListInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ListInit" /> and the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property set to the specified value.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="initializers">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="initializers" /> is null.-or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="newExpression" />.Type does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">There is no instance method named "Add" (case insensitive) declared in <paramref name="newExpression" />.Type or its base type.-or-The add method on <paramref name="newExpression" />.Type or its base type does not take exactly one argument.-or-The type represented by the <see cref="P:System.Linq.Expressions.Expression.Type" /> property of the first element of <paramref name="initializers" /> is not assignable to the argument type of the add method on <paramref name="newExpression" />.Type or its base type.-or-More than one argument-compatible method named "Add" (case-insensitive) exists on <paramref name="newExpression" />.Type and/or its base type.</exception>
        public static ListInitExpression ListInit(
          NewExpression newExpression,
          IEnumerable<Expression> initializers)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            if (!initializers.Any<Expression>())
                throw Error.ListInitializerWithZeroMembers();
            MethodInfo method = Expression.FindMethod(newExpression.Type, "Add", (Type[])null, new Expression[1]
            {
        initializers.First<Expression>()
            }, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return Expression.ListInit(newExpression, method, initializers);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ListInitExpression" /> that uses a specified method to add elements to a collection.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ListInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ListInit" /> and the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property set to the specified value.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="addMethod">A <see cref="T:System.Reflection.MethodInfo" /> that represents an instance method that takes one argument, that adds an element to a collection.</param>
        /// <param name="initializers">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="initializers" /> is null.-or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="newExpression" />.Type does not implement <see cref="T:System.Collections.IEnumerable" />.-or-<paramref name="addMethod" /> is not null and it does not represent an instance method named "Add" (case insensitive) that takes exactly one argument.-or-<paramref name="addMethod" /> is not null and the type represented by the <see cref="P:System.Linq.Expressions.Expression.Type" /> property of one or more elements of <paramref name="initializers" /> is not assignable to the argument type of the method that <paramref name="addMethod" /> represents.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="addMethod" /> is null and no instance method named "Add" that takes one type-compatible argument exists on <paramref name="newExpression" />.Type or its base type.</exception>
        public static ListInitExpression ListInit(
          NewExpression newExpression,
          MethodInfo addMethod,
          params Expression[] initializers)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            return addMethod == null ? Expression.ListInit(newExpression, (IEnumerable<Expression>)initializers) : Expression.ListInit(newExpression, addMethod, (IEnumerable<Expression>)initializers);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ListInitExpression" /> that uses a specified method to add elements to a collection.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ListInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ListInit" /> and the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property set to the specified value.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="addMethod">A <see cref="T:System.Reflection.MethodInfo" /> that represents an instance method named "Add" (case insensitive), that adds an element to a collection.</param>
        /// <param name="initializers">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="initializers" /> is null.-or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="newExpression" />.Type does not implement <see cref="T:System.Collections.IEnumerable" />.-or-<paramref name="addMethod" /> is not null and it does not represent an instance method named "Add" (case insensitive) that takes exactly one argument.-or-<paramref name="addMethod" /> is not null and the type represented by the <see cref="P:System.Linq.Expressions.Expression.Type" /> property of one or more elements of <paramref name="initializers" /> is not assignable to the argument type of the method that <paramref name="addMethod" /> represents.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="addMethod" /> is null and no instance method named "Add" that takes one type-compatible argument exists on <paramref name="newExpression" />.Type or its base type.</exception>
        public static ListInitExpression ListInit(
          NewExpression newExpression,
          MethodInfo addMethod,
          IEnumerable<Expression> initializers)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            if (!initializers.Any<Expression>())
                throw Error.ListInitializerWithZeroMembers();
            if (addMethod == null)
                return Expression.ListInit(newExpression, initializers);
            List<System.Linq.Expressions.ElementInit> initializers1 = new List<System.Linq.Expressions.ElementInit>();
            foreach (Expression initializer in initializers)
                initializers1.Add(Expression.ElementInit(addMethod, initializer));
            return Expression.ListInit(newExpression, (IEnumerable<System.Linq.Expressions.ElementInit>)initializers1);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ListInitExpression" /> that uses specified <see cref="T:System.Linq.Expressions.ElementInit" /> objects to initialize a collection.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ListInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ListInit" /> and the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> and <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> properties set to the specified values.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="initializers">An array of <see cref="T:System.Linq.Expressions.ElementInit" /> objects to use to populate the <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="initializers" /> is null.-or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="newExpression" />.Type does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        public static ListInitExpression ListInit(
          NewExpression newExpression,
          params System.Linq.Expressions.ElementInit[] initializers)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            return initializers != null ? Expression.ListInit(newExpression, (IEnumerable<System.Linq.Expressions.ElementInit>)((IEnumerable<System.Linq.Expressions.ElementInit>)initializers).ToReadOnlyCollection<System.Linq.Expressions.ElementInit>()) : throw Error.ArgumentNull(nameof(initializers));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ListInitExpression" /> that uses specified <see cref="T:System.Linq.Expressions.ElementInit" /> objects to initialize a collection.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ListInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ListInit" /> and the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> and <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> properties set to the specified values.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.ListInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="initializers">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.ElementInit" /> objects to use to populate the <see cref="P:System.Linq.Expressions.ListInitExpression.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="initializers" /> is null.-or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="newExpression" />.Type does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        public static ListInitExpression ListInit(
          NewExpression newExpression,
          IEnumerable<System.Linq.Expressions.ElementInit> initializers)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            ReadOnlyCollection<System.Linq.Expressions.ElementInit> initializers1 = initializers.Any<System.Linq.Expressions.ElementInit>() ? initializers.ToReadOnlyCollection<System.Linq.Expressions.ElementInit>() : throw Error.ListInitializerWithZeroMembers();
            Expression.ValidateListInitArgs(newExpression.Type, initializers1);
            return new ListInitExpression(newExpression, initializers1);
        }

        /// <summary>Creates an <see cref="T:System.Linq.Expressions.ElementInit" />, given an array of values as the second argument.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.ElementInit" /> that has the <see cref="P:System.Linq.Expressions.ElementInit.AddMethod" /> and <see cref="P:System.Linq.Expressions.ElementInit.Arguments" /> properties set to the specified values.</returns>
        /// <param name="addMethod">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.ElementInit.AddMethod" /> property equal to.</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to set the <see cref="P:System.Linq.Expressions.ElementInit.Arguments" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="addMethod" /> or <paramref name="arguments" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The method that <paramref name="addMethod" /> represents is not named "Add" (case insensitive).-or-The method that <paramref name="addMethod" /> represents is not an instance method.-or-<paramref name="arguments" /> does not contain the same number of elements as the number of parameters for the method that <paramref name="addMethod" /> represents.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of one or more elements of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the method that <paramref name="addMethod" /> represents.</exception>
        public static System.Linq.Expressions.ElementInit ElementInit(
          MethodInfo addMethod,
          params Expression[] arguments)
        {
            return Expression.ElementInit(addMethod, (IEnumerable<Expression>)arguments);
        }

        /// <summary>Creates an <see cref="T:System.Linq.Expressions.ElementInit" />, given an <see cref="T:System.Collections.Generic.IEnumerable`1" /> as the second argument.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.ElementInit" /> that has the <see cref="P:System.Linq.Expressions.ElementInit.AddMethod" /> and <see cref="P:System.Linq.Expressions.ElementInit.Arguments" /> properties set to the specified values.</returns>
        /// <param name="addMethod">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.ElementInit.AddMethod" /> property equal to.</param>
        /// <param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to set the <see cref="P:System.Linq.Expressions.ElementInit.Arguments" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="addMethod" /> or <paramref name="arguments" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The method that <paramref name="addMethod" /> represents is not named "Add" (case insensitive).-or-The method that <paramref name="addMethod" /> represents is not an instance method.-or-<paramref name="arguments" /> does not contain the same number of elements as the number of parameters for the method that <paramref name="addMethod" /> represents.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of one or more elements of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the method that <paramref name="addMethod" /> represents.</exception>
        public static System.Linq.Expressions.ElementInit ElementInit(
          MethodInfo addMethod,
          IEnumerable<Expression> arguments)
        {
            if (addMethod == null)
                throw Error.ArgumentNull(nameof(addMethod));
            if (arguments == null)
                throw Error.ArgumentNull(nameof(arguments));
            Expression.ValidateElementInitAddMethodInfo(addMethod);
            ReadOnlyCollection<Expression> readOnlyCollection = arguments.ToReadOnlyCollection<Expression>();
            Expression.ValidateArgumentTypes(addMethod, ref readOnlyCollection);
            return new System.Linq.Expressions.ElementInit(addMethod, readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberListBinding" /> where the member is a field or property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberListBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.ListBinding" /> and the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> and <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> properties set to the specified values.</returns>
        /// <param name="member">A <see cref="T:System.Reflection.MemberInfo" /> that represents a field or property to set the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property equal to.</param>
        /// <param name="initializers">An array of <see cref="T:System.Linq.Expressions.ElementInit" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="member" /> is null. -or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="member" /> does not represent a field or property.-or-The <see cref="P:System.Reflection.FieldInfo.FieldType" /> or <see cref="P:System.Reflection.PropertyInfo.PropertyType" /> of the field or property that <paramref name="member" /> represents does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        public static MemberListBinding ListBind(
          MemberInfo member,
          params System.Linq.Expressions.ElementInit[] initializers)
        {
            if (member == null)
                throw Error.ArgumentNull(nameof(member));
            return initializers != null ? Expression.ListBind(member, (IEnumerable<System.Linq.Expressions.ElementInit>)((IEnumerable<System.Linq.Expressions.ElementInit>)initializers).ToReadOnlyCollection<System.Linq.Expressions.ElementInit>()) : throw Error.ArgumentNull(nameof(initializers));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberListBinding" /> where the member is a field or property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberListBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.ListBinding" /> and the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> and <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> properties set to the specified values.</returns>
        /// <param name="member">A <see cref="T:System.Reflection.MemberInfo" /> that represents a field or property to set the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property equal to.</param>
        /// <param name="initializers">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.ElementInit" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="member" /> is null. -or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="member" /> does not represent a field or property.-or-The <see cref="P:System.Reflection.FieldInfo.FieldType" /> or <see cref="P:System.Reflection.PropertyInfo.PropertyType" /> of the field or property that <paramref name="member" /> represents does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        public static MemberListBinding ListBind(
          MemberInfo member,
          IEnumerable<System.Linq.Expressions.ElementInit> initializers)
        {
            if (member == null)
                throw Error.ArgumentNull(nameof(member));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            Type memberType;
            Expression.ValidateGettableFieldOrPropertyMember(member, out memberType);
            ReadOnlyCollection<System.Linq.Expressions.ElementInit> readOnlyCollection = initializers.ToReadOnlyCollection<System.Linq.Expressions.ElementInit>();
            Expression.ValidateListInitArgs(memberType, readOnlyCollection);
            return new MemberListBinding(member, readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberListBinding" /> object based on a specified property accessor method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberListBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.ListBinding" />, the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property set to the <see cref="T:System.Reflection.MemberInfo" /> that represents the property accessed in <paramref name="propertyAccessor" />, and <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> populated with the elements of <paramref name="initializers" />.</returns>
        /// <param name="propertyAccessor">A <see cref="T:System.Reflection.MethodInfo" /> that represents a property accessor method.</param>
        /// <param name="initializers">An array of <see cref="T:System.Linq.Expressions.ElementInit" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="propertyAccessor" /> is null. -or-One or more elements of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="propertyAccessor" /> does not represent a property accessor method.-or-The <see cref="P:System.Reflection.PropertyInfo.PropertyType" /> of the property that the method represented by <paramref name="propertyAccessor" /> accesses does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        public static MemberListBinding ListBind(
          MethodInfo propertyAccessor,
          params System.Linq.Expressions.ElementInit[] initializers)
        {
            if (propertyAccessor == null)
                throw Error.ArgumentNull(nameof(propertyAccessor));
            return initializers != null ? Expression.ListBind(propertyAccessor, (IEnumerable<System.Linq.Expressions.ElementInit>)((IEnumerable<System.Linq.Expressions.ElementInit>)initializers).ToReadOnlyCollection<System.Linq.Expressions.ElementInit>()) : throw Error.ArgumentNull(nameof(initializers));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberListBinding" /> based on a specified property accessor method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberListBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.ListBinding" />, the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property set to the <see cref="T:System.Reflection.MemberInfo" /> that represents the property accessed in <paramref name="propertyAccessor" />, and <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> populated with the elements of <paramref name="initializers" />.</returns>
        /// <param name="propertyAccessor">A <see cref="T:System.Reflection.MethodInfo" /> that represents a property accessor method.</param>
        /// <param name="initializers">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.ElementInit" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberListBinding.Initializers" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="propertyAccessor" /> is null. -or-One or more elements of <paramref name="initializers" /> are null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="propertyAccessor" /> does not represent a property accessor method.-or-The <see cref="P:System.Reflection.PropertyInfo.PropertyType" /> of the property that the method represented by <paramref name="propertyAccessor" /> accesses does not implement <see cref="T:System.Collections.IEnumerable" />.</exception>
        public static MemberListBinding ListBind(
          MethodInfo propertyAccessor,
          IEnumerable<System.Linq.Expressions.ElementInit> initializers)
        {
            if (propertyAccessor == null)
                throw Error.ArgumentNull(nameof(propertyAccessor));
            return initializers != null ? Expression.ListBind((MemberInfo)Expression.GetProperty(propertyAccessor), initializers) : throw Error.ArgumentNull(nameof(initializers));
        }

        private static void ValidateListInitArgs(
          Type listType,
          ReadOnlyCollection<System.Linq.Expressions.ElementInit> initializers)
        {
            if (!Expression.AreAssignable(typeof(IEnumerable), listType))
                throw Error.TypeNotIEnumerable((object)listType);
            int index = 0;
            for (int count = initializers.Count; index < count; ++index)
                Expression.ValidateCallInstanceType(listType, (initializers[index] ?? throw Error.ArgumentNull(nameof(initializers))).AddMethod);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberInitExpression" />.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberInit" /> and the <see cref="P:System.Linq.Expressions.MemberInitExpression.NewExpression" /> and <see cref="P:System.Linq.Expressions.MemberInitExpression.Bindings" /> properties set to the specified values.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.MemberInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="bindings">An array of <see cref="T:System.Linq.Expressions.MemberBinding" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberInitExpression.Bindings" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="bindings" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property of an element of <paramref name="bindings" /> does not represent a member of the type that <paramref name="newExpression" />.Type represents.</exception>
        public static MemberInitExpression MemberInit(
          NewExpression newExpression,
          params MemberBinding[] bindings)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            return bindings != null ? Expression.MemberInit(newExpression, (IEnumerable<MemberBinding>)((IEnumerable<MemberBinding>)bindings).ToReadOnlyCollection<MemberBinding>()) : throw Error.ArgumentNull(nameof(bindings));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberInitExpression" />.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberInitExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberInit" /> and the <see cref="P:System.Linq.Expressions.MemberInitExpression.NewExpression" /> and <see cref="P:System.Linq.Expressions.MemberInitExpression.Bindings" /> properties set to the specified values.</returns>
        /// <param name="newExpression">A <see cref="T:System.Linq.Expressions.NewExpression" /> to set the <see cref="P:System.Linq.Expressions.MemberInitExpression.NewExpression" /> property equal to.</param>
        /// <param name="bindings">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.MemberBinding" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberInitExpression.Bindings" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="newExpression" /> or <paramref name="bindings" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property of an element of <paramref name="bindings" /> does not represent a member of the type that <paramref name="newExpression" />.Type represents.</exception>
        public static MemberInitExpression MemberInit(
          NewExpression newExpression,
          IEnumerable<MemberBinding> bindings)
        {
            if (newExpression == null)
                throw Error.ArgumentNull(nameof(newExpression));
            ReadOnlyCollection<MemberBinding> bindings1 = bindings != null ? bindings.ToReadOnlyCollection<MemberBinding>() : throw Error.ArgumentNull(nameof(bindings));
            Expression.ValidateMemberInitArgs(newExpression.Type, bindings1);
            return new MemberInitExpression(newExpression, bindings1);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that represents the recursive initialization of members of a field or property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.MemberBinding" /> and the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> and <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> properties set to the specified values.</returns>
        /// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to set the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property equal to.</param>
        /// <param name="bindings">An array of <see cref="T:System.Linq.Expressions.MemberBinding" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="member" /> or <paramref name="bindings" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="member" /> does not represent a field or property.-or-The <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property of an element of <paramref name="bindings" /> does not represent a member of the type of the field or property that <paramref name="member" /> represents.</exception>
        public static MemberMemberBinding MemberBind(
          MemberInfo member,
          params MemberBinding[] bindings)
        {
            if (member == null)
                throw Error.ArgumentNull(nameof(member));
            return bindings != null ? Expression.MemberBind(member, (IEnumerable<MemberBinding>)((IEnumerable<MemberBinding>)bindings).ToReadOnlyCollection<MemberBinding>()) : throw Error.ArgumentNull(nameof(bindings));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that represents the recursive initialization of members of a field or property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.MemberBinding" /> and the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> and <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> properties set to the specified values.</returns>
        /// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to set the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property equal to.</param>
        /// <param name="bindings">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.MemberBinding" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="member" /> or <paramref name="bindings" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="member" /> does not represent a field or property.-or-The <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property of an element of <paramref name="bindings" /> does not represent a member of the type of the field or property that <paramref name="member" /> represents.</exception>
        public static MemberMemberBinding MemberBind(
          MemberInfo member,
          IEnumerable<MemberBinding> bindings)
        {
            if (member == null)
                throw Error.ArgumentNull(nameof(member));
            ReadOnlyCollection<MemberBinding> bindings1 = bindings != null ? bindings.ToReadOnlyCollection<MemberBinding>() : throw Error.ArgumentNull(nameof(bindings));
            Type memberType;
            Expression.ValidateGettableFieldOrPropertyMember(member, out memberType);
            Expression.ValidateMemberInitArgs(memberType, bindings1);
            return new MemberMemberBinding(member, bindings1);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that represents the recursive initialization of members of a member that is accessed by using a property accessor method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.MemberBinding" />, the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property set to the <see cref="T:System.Reflection.PropertyInfo" /> that represents the property accessed in <paramref name="propertyAccessor" />, and <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> properties set to the specified values.</returns>
        /// <param name="propertyAccessor">The <see cref="T:System.Reflection.MethodInfo" /> that represents a property accessor method.</param>
        /// <param name="bindings">An array of <see cref="T:System.Linq.Expressions.MemberBinding" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="propertyAccessor" /> or <paramref name="bindings" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="propertyAccessor" /> does not represent a property accessor method.-or-The <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property of an element of <paramref name="bindings" /> does not represent a member of the type of the property accessed by the method that <paramref name="propertyAccessor" /> represents.</exception>
        public static MemberMemberBinding MemberBind(
          MethodInfo propertyAccessor,
          params MemberBinding[] bindings)
        {
            return propertyAccessor != null ? Expression.MemberBind((MemberInfo)Expression.GetProperty(propertyAccessor), bindings) : throw Error.ArgumentNull(nameof(propertyAccessor));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that represents the recursive initialization of members of a member that is accessed by using a property accessor method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberMemberBinding" /> that has the <see cref="P:System.Linq.Expressions.MemberBinding.BindingType" /> property equal to <see cref="F:System.Linq.Expressions.MemberBindingType.MemberBinding" />, the <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property set to the <see cref="T:System.Reflection.PropertyInfo" /> that represents the property accessed in <paramref name="propertyAccessor" />, and <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> properties set to the specified values.</returns>
        /// <param name="propertyAccessor">The <see cref="T:System.Reflection.MethodInfo" /> that represents a property accessor method.</param>
        /// <param name="bindings">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.MemberBinding" /> objects to use to populate the <see cref="P:System.Linq.Expressions.MemberMemberBinding.Bindings" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="propertyAccessor" /> or <paramref name="bindings" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="propertyAccessor" /> does not represent a property accessor method.-or-The <see cref="P:System.Linq.Expressions.MemberBinding.Member" /> property of an element of <paramref name="bindings" /> does not represent a member of the type of the property accessed by the method that <paramref name="propertyAccessor" /> represents.</exception>
        public static MemberMemberBinding MemberBind(
          MethodInfo propertyAccessor,
          IEnumerable<MemberBinding> bindings)
        {
            return propertyAccessor != null ? Expression.MemberBind((MemberInfo)Expression.GetProperty(propertyAccessor), bindings) : throw Error.ArgumentNull(nameof(propertyAccessor));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic remainder operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Modulo" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The modulus operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Modulo(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.Modulo, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Modulo, "op_Modulus", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic remainder operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Modulo" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the modulus operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Modulo(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Modulo(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Modulo, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic multiplication operation that does not have overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Multiply" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The multiplication operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Multiply(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.Multiply, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Multiply, "op_Multiply", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic multiplication operation that does not have overflow checking and for which the implementing method is specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Multiply" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the multiplication operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Multiply(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Multiply(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Multiply, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic multiplication operation that has overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MultiplyChecked" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The multiplication operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression MultiplyChecked(
          Expression left,
          Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.MultiplyChecked, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.MultiplyChecked, "op_Multiply", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic multiplication operation that has overflow checking. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MultiplyChecked" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the multiplication operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression MultiplyChecked(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.MultiplyChecked(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.MultiplyChecked, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a unary plus operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.UnaryPlus" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property set to the specified value.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The unary plus operator is not defined for <paramref name="expression" />.Type.</exception>
        public static UnaryExpression UnaryPlus(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return Expression.IsArithmetic(expression.Type) ? new UnaryExpression(ExpressionType.UnaryPlus, expression, expression.Type) : Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.UnaryPlus, "op_UnaryPlus", expression);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a unary plus operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.UnaryPlus" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly one argument.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the unary plus operator is not defined for <paramref name="expression" />.Type.-or-<paramref name="expression" />.Type (or its corresponding non-nullable type if it is a nullable value type) is not assignable to the argument type of the method represented by <paramref name="method" />.</exception>
        public static UnaryExpression UnaryPlus(Expression expression, MethodInfo method)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return method == null ? Expression.UnaryPlus(expression) : Expression.GetMethodBasedUnaryOperator(ExpressionType.UnaryPlus, expression, method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents an arithmetic negation operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Negate" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property set to the specified value.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The unary minus operator is not defined for <paramref name="expression" />.Type.</exception>
        public static UnaryExpression Negate(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return Expression.IsArithmetic(expression.Type) && !Expression.IsUnSigned(expression.Type) ? new UnaryExpression(ExpressionType.Negate, expression, expression.Type) : Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Negate, "op_UnaryNegation", expression);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents an arithmetic negation operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Negate" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly one argument.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the unary minus operator is not defined for <paramref name="expression" />.Type.-or-<paramref name="expression" />.Type (or its corresponding non-nullable type if it is a nullable value type) is not assignable to the argument type of the method represented by <paramref name="method" />.</exception>
        public static UnaryExpression Negate(Expression expression, MethodInfo method)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return method == null ? Expression.Negate(expression) : Expression.GetMethodBasedUnaryOperator(ExpressionType.Negate, expression, method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents an arithmetic negation operation that has overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NegateChecked" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property set to the specified value.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The unary minus operator is not defined for <paramref name="expression" />.Type.</exception>
        public static UnaryExpression NegateChecked(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return Expression.IsArithmetic(expression.Type) && !Expression.IsUnSigned(expression.Type) ? new UnaryExpression(ExpressionType.NegateChecked, expression, expression.Type) : Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.NegateChecked, "op_UnaryNegation", expression);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents an arithmetic negation operation that has overflow checking. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NegateChecked" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly one argument.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the unary minus operator is not defined for <paramref name="expression" />.Type.-or-<paramref name="expression" />.Type (or its corresponding non-nullable type if it is a nullable value type) is not assignable to the argument type of the method represented by <paramref name="method" />.</exception>
        public static UnaryExpression NegateChecked(
          Expression expression,
          MethodInfo method)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return method == null ? Expression.NegateChecked(expression) : Expression.GetMethodBasedUnaryOperator(ExpressionType.NegateChecked, expression, method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an inequality comparison.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NotEqual" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The inequality operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression NotEqual(Expression left, Expression right) => Expression.NotEqual(left, right, false, (MethodInfo)null);

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an inequality comparison. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NotEqual" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the inequality operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression NotEqual(
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.GetEqualityComparisonOperator(ExpressionType.NotEqual, "op_Inequality", left, right, liftToNull) : Expression.GetMethodBasedBinaryOperator(ExpressionType.NotEqual, left, right, method, liftToNull);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewExpression" /> that represents calling the specified constructor with the specified arguments.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.New" /> and the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> and <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="constructor">The <see cref="T:System.Reflection.ConstructorInfo" /> to set the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property equal to.</param>
        /// <param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="constructor" /> is null.-or-An element of <paramref name="arguments" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The length of <paramref name="arguments" /> does match the number of parameters for the constructor that <paramref name="constructor" /> represents.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the constructor that <paramref name="constructor" /> represents.</exception>
        public static NewExpression New(
          ConstructorInfo constructor,
          params Expression[] arguments)
        {
            return Expression.New(constructor, (IEnumerable<Expression>)((IEnumerable<Expression>)arguments).ToReadOnlyCollection<Expression>());
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewExpression" /> that represents calling the specified constructor with the specified arguments.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.New" /> and the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> and <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> properties set to the specified values.</returns>
        /// <param name="constructor">The <see cref="T:System.Reflection.ConstructorInfo" /> to set the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property equal to.</param>
        /// <param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="constructor" /> is null.-or-An element of <paramref name="arguments" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="arguments" /> parameter does not contain the same number of elements as the number of parameters for the constructor that <paramref name="constructor" /> represents.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the constructor that <paramref name="constructor" /> represents.</exception>
        public static NewExpression New(
          ConstructorInfo constructor,
          IEnumerable<Expression> arguments)
        {
            if (constructor == null)
                throw Error.ArgumentNull(nameof(constructor));
            ReadOnlyCollection<Expression> readOnlyCollection = arguments.ToReadOnlyCollection<Expression>();
            Expression.ValidateNewArgs(constructor.DeclaringType, constructor, ref readOnlyCollection);
            return new NewExpression(constructor.DeclaringType, constructor, readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewExpression" /> that represents calling the specified constructor with the specified arguments. The members that access the constructor initialized fields are specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.New" /> and the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" />, <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> and <see cref="P:System.Linq.Expressions.NewExpression.Members" /> properties set to the specified values.</returns>
        /// <param name="constructor">The <see cref="T:System.Reflection.ConstructorInfo" /> to set the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property equal to.</param>
        /// <param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> collection.</param>
        /// <param name="members">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Reflection.MemberInfo" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewExpression.Members" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="constructor" /> is null.-or-An element of <paramref name="arguments" /> is null.-or-An element of <paramref name="members" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="arguments" /> parameter does not contain the same number of elements as the number of parameters for the constructor that <paramref name="constructor" /> represents.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the constructor that <paramref name="constructor" /> represents.-or-The <paramref name="members" /> parameter does not have the same number of elements as <paramref name="arguments" />.-or-An element of <paramref name="arguments" /> has a <see cref="P:System.Linq.Expressions.Expression.Type" /> property that represents a type that is not assignable to the type of the member that is represented by the corresponding element of <paramref name="members" />.-or-An element of <paramref name="members" /> represents a property that does not have a get accessor.</exception>
        public static NewExpression New(
          ConstructorInfo constructor,
          IEnumerable<Expression> arguments,
          IEnumerable<MemberInfo> members)
        {
            if (constructor == null)
                throw Error.ArgumentNull(nameof(constructor));
            ReadOnlyCollection<MemberInfo> readOnlyCollection1 = members.ToReadOnlyCollection<MemberInfo>();
            ReadOnlyCollection<Expression> readOnlyCollection2 = arguments.ToReadOnlyCollection<Expression>();
            Expression.ValidateNewArgs(constructor, ref readOnlyCollection2, readOnlyCollection1);
            return new NewExpression(constructor.DeclaringType, constructor, readOnlyCollection2, readOnlyCollection1);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewExpression" /> that represents calling the specified constructor with the specified arguments. The members that access the constructor initialized fields are specified as an array.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.New" /> and the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" />, <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> and <see cref="P:System.Linq.Expressions.NewExpression.Members" /> properties set to the specified values.</returns>
        /// <param name="constructor">The <see cref="T:System.Reflection.ConstructorInfo" /> to set the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property equal to.</param>
        /// <param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewExpression.Arguments" /> collection.</param>
        /// <param name="members">An array of <see cref="T:System.Reflection.MemberInfo" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewExpression.Members" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="constructor" /> is null.-or-An element of <paramref name="arguments" /> is null.-or-An element of <paramref name="members" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="arguments" /> parameter does not contain the same number of elements as the number of parameters for the constructor that <paramref name="constructor" /> represents.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the constructor that <paramref name="constructor" /> represents.-or-The <paramref name="members" /> parameter does not have the same number of elements as <paramref name="arguments" />.-or-An element of <paramref name="arguments" /> has a <see cref="P:System.Linq.Expressions.Expression.Type" /> property that represents a type that is not assignable to the type of the member that is represented by the corresponding element of <paramref name="members" />.-or-An element of <paramref name="members" /> represents a property that does not have a get accessor.</exception>
        public static NewExpression New(
          ConstructorInfo constructor,
          IEnumerable<Expression> arguments,
          params MemberInfo[] members)
        {
            return Expression.New(constructor, arguments, (IEnumerable<MemberInfo>)((IEnumerable<MemberInfo>)members).ToReadOnlyCollection<MemberInfo>());
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewExpression" /> that represents calling the specified constructor that takes no arguments.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.New" /> and the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property set to the specified value.</returns>
        /// <param name="constructor">The <see cref="T:System.Reflection.ConstructorInfo" /> to set the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="constructor" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The constructor that <paramref name="constructor" /> represents has at least one parameter.</exception>
        public static NewExpression New(ConstructorInfo constructor) => Expression.New(constructor, (IEnumerable<Expression>)((IEnumerable<Expression>)null).ToReadOnlyCollection<Expression>());

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewExpression" /> that represents calling the parameterless constructor of the specified type.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.New" /> and the <see cref="P:System.Linq.Expressions.NewExpression.Constructor" /> property set to the <see cref="T:System.Reflection.ConstructorInfo" /> that represents the parameterless constructor of the specified type.</returns>
        /// <param name="type">A <see cref="T:System.Type" /> that has a constructor that takes no arguments.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The type that <paramref name="type" /> represents does not have a parameterless constructor.</exception>
        public static NewExpression New(Type type)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (type == typeof(void))
                throw Error.ArgumentCannotBeOfTypeVoid();
            if (!type.IsValueType)
                return Expression.New(type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, Type.EmptyTypes, (ParameterModifier[])null) ?? throw Error.TypeMissingDefaultConstructor((object)type));
            ReadOnlyCollection<Expression> readOnlyCollection = ((IEnumerable<Expression>)null).ToReadOnlyCollection<Expression>();
            return new NewExpression(type, (ConstructorInfo)null, readOnlyCollection);
        }

        private static void ValidateNewArgs(
          ConstructorInfo constructor,
          ref ReadOnlyCollection<Expression> arguments,
          ReadOnlyCollection<MemberInfo> members)
        {
            ParameterInfo[] parameters;
            if ((parameters = constructor.GetParameters()).Length > 0)
            {
                if (arguments.Count != parameters.Length)
                    throw Error.IncorrectNumberOfConstructorArguments();
                if (arguments.Count != members.Count)
                    throw Error.IncorrectNumberOfArgumentsForMembers();
                List<Expression> sequence = (List<Expression>)null;
                int index1 = 0;
                for (int count = arguments.Count; index1 < count; ++index1)
                {
                    Expression expression = arguments[index1];
                    if (expression == null)
                        throw Error.ArgumentNull("argument");
                    MemberInfo member = members[index1];
                    if (member == null)
                        throw Error.ArgumentNull("member");
                    if (member.DeclaringType != constructor.DeclaringType)
                        throw Error.ArgumentMemberNotDeclOnType((object)member.Name, (object)constructor.DeclaringType.Name);
                    Type memberType;
                    Expression.ValidateAnonymousTypeMember(member, out memberType);
                    if (!Expression.AreReferenceAssignable(expression.Type, memberType))
                        expression = Expression.IsSameOrSubclass(typeof(Expression), memberType) && Expression.AreAssignable(memberType, expression.GetType()) ? (Expression)Expression.Quote(expression) : throw Error.ArgumentTypeDoesNotMatchMember((object)expression.Type, (object)memberType);
                    Type type = parameters[index1].ParameterType;
                    if (type.IsByRef)
                        type = type.GetElementType();
                    if (!Expression.AreReferenceAssignable(type, expression.Type))
                    {
                        if (!Expression.IsSameOrSubclass(typeof(Expression), type) || !Expression.AreAssignable(type, expression.Type))
                            throw Error.ExpressionTypeDoesNotMatchConstructorParameter((object)expression.Type, (object)type);
                        expression = (Expression)Expression.Quote(expression);
                    }
                    if (sequence == null && expression != arguments[index1])
                    {
                        sequence = new List<Expression>(arguments.Count);
                        for (int index2 = 0; index2 < index1; ++index2)
                            sequence.Add(arguments[index2]);
                    }
                    sequence?.Add(expression);
                }
                if (sequence == null)
                    return;
                arguments = sequence.ToReadOnlyCollection<Expression>();
            }
            else
            {
                if (arguments != null && arguments.Count > 0)
                    throw Error.IncorrectNumberOfConstructorArguments();
                if (members != null && members.Count > 0)
                    throw Error.IncorrectNumberOfMembersForGivenConstructor();
            }
        }

        private static void ValidateNewArgs(
          Type type,
          ConstructorInfo constructor,
          ref ReadOnlyCollection<Expression> arguments)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (!type.IsValueType && constructor == null)
                throw Error.ArgumentNull(nameof(constructor));
            ParameterInfo[] parameters;
            if (constructor != null && (parameters = constructor.GetParameters()).Length > 0)
            {
                if (arguments.Count != parameters.Length)
                    throw Error.IncorrectNumberOfConstructorArguments();
                List<Expression> sequence = (List<Expression>)null;
                int index1 = 0;
                for (int count = arguments.Count; index1 < count; ++index1)
                {
                    Expression expression = arguments[index1];
                    ParameterInfo parameterInfo = parameters[index1];
                    if (expression == null)
                        throw Error.ArgumentNull(nameof(arguments));
                    Type type1 = parameterInfo.ParameterType;
                    if (type1.IsByRef)
                        type1 = type1.GetElementType();
                    if (!Expression.AreReferenceAssignable(type1, expression.Type))
                        expression = Expression.IsSameOrSubclass(typeof(Expression), type1) && Expression.AreAssignable(type1, expression.GetType()) ? (Expression)Expression.Quote(expression) : throw Error.ExpressionTypeDoesNotMatchConstructorParameter((object)expression.Type, (object)type1);
                    if (sequence == null && expression != arguments[index1])
                    {
                        sequence = new List<Expression>(arguments.Count);
                        for (int index2 = 0; index2 < index1; ++index2)
                            sequence.Add(arguments[index2]);
                    }
                    sequence?.Add(expression);
                }
                if (sequence == null)
                    return;
                arguments = sequence.ToReadOnlyCollection<Expression>();
            }
            else if (arguments != null && arguments.Count > 0)
                throw Error.IncorrectNumberOfConstructorArguments();
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that represents creating an array that has a specified rank.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NewArrayBounds" /> and the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> property set to the specified value.</returns>
        /// <param name="type">A <see cref="T:System.Type" /> that represents the element type of the array.</param>
        /// <param name="bounds">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="bounds" /> is null.-or-An element of <paramref name="bounds" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="bounds" /> does not represent an integral type.</exception>
        public static NewArrayExpression NewArrayBounds(
          Type type,
          params Expression[] bounds)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (bounds == null)
                throw Error.ArgumentNull(nameof(bounds));
            return !type.Equals(typeof(void)) ? Expression.NewArrayBounds(type, (IEnumerable<Expression>)((IEnumerable<Expression>)bounds).ToReadOnlyCollection<Expression>()) : throw Error.ArgumentCannotBeOfTypeVoid();
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that represents creating an array that has a specified rank.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NewArrayBounds" /> and the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> property set to the specified value.</returns>
        /// <param name="type">A <see cref="T:System.Type" /> that represents the element type of the array.</param>
        /// <param name="bounds">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="bounds" /> is null.-or-An element of <paramref name="bounds" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="bounds" /> does not represent an integral type.</exception>
        public static NewArrayExpression NewArrayBounds(
          Type type,
          IEnumerable<Expression> bounds)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (bounds == null)
                throw Error.ArgumentNull(nameof(bounds));
            if (type.Equals(typeof(void)))
                throw Error.ArgumentCannotBeOfTypeVoid();
            ReadOnlyCollection<Expression> readOnlyCollection = bounds.ToReadOnlyCollection<Expression>();
            int index = 0;
            for (int count = readOnlyCollection.Count; index < count; ++index)
                Expression.ValidateIntegerArg((readOnlyCollection[index] ?? throw Error.ArgumentNull(nameof(bounds))).Type);
            return new NewArrayExpression(ExpressionType.NewArrayBounds, type.MakeArrayType(readOnlyCollection.Count), readOnlyCollection);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that represents creating a one-dimensional array and initializing it from a list of elements.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NewArrayInit" /> and the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> property set to the specified value.</returns>
        /// <param name="type">A <see cref="T:System.Type" /> that represents the element type of the array.</param>
        /// <param name="initializers">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="initializers" /> is null.-or-An element of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="initializers" /> represents a type that is not assignable to the type <paramref name="type" />.</exception>
        public static NewArrayExpression NewArrayInit(
          Type type,
          params Expression[] initializers)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            return !type.Equals(typeof(void)) ? Expression.NewArrayInit(type, (IEnumerable<Expression>)((IEnumerable<Expression>)initializers).ToReadOnlyCollection<Expression>()) : throw Error.ArgumentCannotBeOfTypeVoid();
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that represents creating a one-dimensional array and initializing it from a list of elements.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.NewArrayExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.NewArrayInit" /> and the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> property set to the specified value.</returns>
        /// <param name="type">A <see cref="T:System.Type" /> that represents the element type of the array.</param>
        /// <param name="initializers">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.NewArrayExpression.Expressions" /> collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> or <paramref name="initializers" /> is null.-or-An element of <paramref name="initializers" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="initializers" /> represents a type that is not assignable to the type that <paramref name="type" /> represents.</exception>
        public static NewArrayExpression NewArrayInit(
          Type type,
          IEnumerable<Expression> initializers)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (initializers == null)
                throw Error.ArgumentNull(nameof(initializers));
            if (type.Equals(typeof(void)))
                throw Error.ArgumentCannotBeOfTypeVoid();
            ReadOnlyCollection<Expression> readOnlyCollection = initializers.ToReadOnlyCollection<Expression>();
            List<Expression> sequence = (List<Expression>)null;
            int index1 = 0;
            for (int count = readOnlyCollection.Count; index1 < count; ++index1)
            {
                Expression expression = readOnlyCollection[index1];
                if (expression == null)
                    throw Error.ArgumentNull(nameof(initializers));
                if (!Expression.AreReferenceAssignable(type, expression.Type))
                    expression = Expression.IsSameOrSubclass(typeof(Expression), type) && Expression.AreAssignable(type, expression.GetType()) ? (Expression)Expression.Quote(expression) : throw Error.ExpressionTypeCannotInitializeArrayType((object)expression.Type, (object)type);
                if (sequence == null && expression != readOnlyCollection[index1])
                {
                    sequence = new List<Expression>(readOnlyCollection.Count);
                    for (int index2 = 0; index2 < index1; ++index2)
                        sequence.Add(readOnlyCollection[index2]);
                }
                sequence?.Add(expression);
            }
            if (sequence != null)
                readOnlyCollection = sequence.ToReadOnlyCollection<Expression>();
            return new NewArrayExpression(ExpressionType.NewArrayInit, type.MakeArrayType(), readOnlyCollection);
        }

        private static void ValidateSettableFieldOrPropertyMember(
          MemberInfo member,
          out Type memberType)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    memberType = fieldInfo.FieldType;
                    break;
                case PropertyInfo p0:
                    memberType = p0.CanWrite ? p0.PropertyType : throw Error.PropertyDoesNotHaveSetter((object)p0);
                    break;
                default:
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfo();
            }
        }

        private static void ValidateAnonymousTypeMember(MemberInfo member, out Type memberType)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = member as FieldInfo;
                    memberType = !fieldInfo.IsStatic ? fieldInfo.FieldType : throw Error.ArgumentMustBeInstanceMember();
                    break;
                case MemberTypes.Method:
                    MethodInfo methodInfo = member as MethodInfo;
                    memberType = !methodInfo.IsStatic ? methodInfo.ReturnType : throw Error.ArgumentMustBeInstanceMember();
                    break;
                case MemberTypes.Property:
                    PropertyInfo p0 = member as PropertyInfo;
                    if (!p0.CanRead)
                        throw Error.PropertyDoesNotHaveGetter((object)p0);
                    memberType = !p0.GetGetMethod().IsStatic ? p0.PropertyType : throw Error.ArgumentMustBeInstanceMember();
                    break;
                default:
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfoOrMethod();
            }
        }

        private static void ValidateGettableFieldOrPropertyMember(
          MemberInfo member,
          out Type memberType)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    memberType = fieldInfo.FieldType;
                    break;
                case PropertyInfo p0:
                    memberType = p0.CanRead ? p0.PropertyType : throw Error.PropertyDoesNotHaveGetter((object)p0);
                    break;
                default:
                    throw Error.ArgumentMustBeFieldInfoOrPropertInfo();
            }
        }

        private static void ValidateMemberInitArgs(
          Type type,
          ReadOnlyCollection<MemberBinding> bindings)
        {
            int index = 0;
            for (int count = bindings.Count; index < count; ++index)
            {
                MemberBinding binding = bindings[index];
                if (!Expression.AreAssignable(binding.Member.DeclaringType, type))
                    throw Error.NotAMemberOfType((object)binding.Member.Name, (object)type);
            }
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a bitwise complement operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Not" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property set to the specified value.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The unary not operator is not defined for <paramref name="expression" />.Type.</exception>
        public static UnaryExpression Not(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return Expression.IsIntegerOrBool(expression.Type) ? new UnaryExpression(ExpressionType.Not, expression, expression.Type) : Expression.GetUserDefinedUnaryOperator(ExpressionType.Not, "op_LogicalNot", expression) ?? Expression.GetUserDefinedUnaryOperatorOrThrow(ExpressionType.Not, "op_OnesComplement", expression);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents a bitwise complement operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Not" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly one argument.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the unary not operator is not defined for <paramref name="expression" />.Type.-or-<paramref name="expression" />.Type (or its corresponding non-nullable type if it is a nullable value type) is not assignable to the argument type of the method represented by <paramref name="method" />.</exception>
        public static UnaryExpression Not(Expression expression, MethodInfo method)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return method == null ? Expression.Not(expression) : Expression.GetMethodBasedUnaryOperator(ExpressionType.Not, expression, method);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise OR operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Or" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The bitwise OR operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Or(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsIntegerOrBool(left.Type) ? new BinaryExpression(ExpressionType.Or, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Or, "op_BitwiseOr", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise OR operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Or" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the bitwise OR operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Or(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Or(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Or, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a conditional OR operation that evaluates the second operand only if it has to.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.OrElse" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The bitwise OR operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.-or-<paramref name="left" />.Type and <paramref name="right" />.Type are not the same Boolean type.</exception>
        public static BinaryExpression OrElse(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            if (left.Type == right.Type && Expression.IsBool(left.Type))
                return new BinaryExpression(ExpressionType.OrElse, left, right, left.Type);
            MethodInfo definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(ExpressionType.OrElse, left.Type, right.Type, "op_BitwiseOr");
            if (definedBinaryOperator == null)
                throw Error.BinaryOperatorNotDefined((object)ExpressionType.OrElse, (object)left.Type, (object)right.Type);
            Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, definedBinaryOperator);
            Type type = !Expression.IsNullableType(left.Type) || definedBinaryOperator.ReturnType != Expression.GetNonNullableType(left.Type) ? definedBinaryOperator.ReturnType : left.Type;
            return new BinaryExpression(ExpressionType.OrElse, left, right, definedBinaryOperator, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a conditional OR operation that evaluates the second operand only if it has to. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.OrElse" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the bitwise OR operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.-or-<paramref name="method" /> is null and <paramref name="left" />.Type and <paramref name="right" />.Type are not the same Boolean type.</exception>
        public static BinaryExpression OrElse(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            if (method == null)
                return Expression.OrElse(left, right);
            Expression.ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.Type, right.Type, method);
            Type type = !Expression.IsNullableType(left.Type) || method.ReturnType != Expression.GetNonNullableType(left.Type) ? method.ReturnType : left.Type;
            return new BinaryExpression(ExpressionType.OrElse, left, right, method, type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.ParameterExpression" />.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.ParameterExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Parameter" /> and the <see cref="P:System.Linq.Expressions.Expression.Type" /> and <see cref="P:System.Linq.Expressions.ParameterExpression.Name" /> properties set to the specified values.</returns>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <param name="name">The value to set the <see cref="P:System.Linq.Expressions.ParameterExpression.Name" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="type" /> is null.</exception>
        public static ParameterExpression Parameter(Type type, string name)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            return type != typeof(void) ? new ParameterExpression(type, name) : throw Error.ArgumentCannotBeOfTypeVoid();
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents raising a number to a power.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Power" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The exponentiation operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.-or-<paramref name="left" />.Type and/or <paramref name="right" />.Type are not <see cref="T:System.Double" />.</exception>
        public static BinaryExpression Power(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return Expression.Power(left, right, typeof(Math).GetMethod("Pow", BindingFlags.Static | BindingFlags.Public) ?? throw Error.BinaryOperatorNotDefined((object)ExpressionType.Power, (object)left.Type, (object)right.Type));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents raising a number to a power. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Power" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the exponentiation operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.-or-<paramref name="method" /> is null and <paramref name="left" />.Type and/or <paramref name="right" />.Type are not <see cref="T:System.Double" />.</exception>
        public static BinaryExpression Power(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Power(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Power, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing a property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberAccess" /> and the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> and <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property equal to.</param>
        /// <param name="property">The <see cref="T:System.Reflection.PropertyInfo" /> to set the <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="property" /> is null.-or-The property that <paramref name="property" /> represents is not static (Shared in Visual Basic) and <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="expression" />.Type is not assignable to the declaring type of the property that <paramref name="property" /> represents.</exception>
        public static MemberExpression Property(
          Expression expression,
          PropertyInfo property)
        {
            if (property == null)
                throw Error.ArgumentNull(nameof(property));
            if (!property.CanRead)
                throw Error.PropertyDoesNotHaveGetter((object)property);
            if (!property.GetGetMethod(true).IsStatic)
            {
                if (expression == null)
                    throw Error.ArgumentNull(nameof(expression));
                if (!Expression.AreReferenceAssignable(property.DeclaringType, expression.Type))
                    throw Error.PropertyNotDefinedForType((object)property, (object)expression.Type);
            }
            return new MemberExpression(expression, (MemberInfo)property, property.PropertyType);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing a property by using a property accessor method.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberAccess" />, the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property set to <paramref name="expression" /> and the <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> property set to the <see cref="T:System.Reflection.PropertyInfo" /> that represents the property accessed in <paramref name="propertyAccessor" />.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property equal to.</param>
        /// <param name="propertyAccessor">The <see cref="T:System.Reflection.MethodInfo" /> that represents a property accessor method.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="propertyAccessor" /> is null.-or-The method that <paramref name="propertyAccessor" /> represents is not static (Shared in Visual Basic) and <paramref name="expression" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="expression" />.Type is not assignable to the declaring type of the method represented by <paramref name="propertyAccessor" />.-or-The method that <paramref name="propertyAccessor" /> represents is not a property accessor method.</exception>
        public static MemberExpression Property(
          Expression expression,
          MethodInfo propertyAccessor)
        {
            if (propertyAccessor == null)
                throw Error.ArgumentNull(nameof(propertyAccessor));
            Expression.ValidateMethodInfo(propertyAccessor);
            return Expression.Property(expression, Expression.GetProperty(propertyAccessor));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing a property given the name of the property.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberAccess" />, the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property set to <paramref name="expression" />, and the <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> property set to the <see cref="T:System.Reflection.PropertyInfo" /> that represents the property denoted by <paramref name="propertyName" />.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> whose <see cref="P:System.Linq.Expressions.Expression.Type" /> contains a property named <paramref name="propertyName" />.</param>
        /// <param name="propertyName">The name of a property.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="propertyName" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">No property named <paramref name="propertyName" /> is defined in <paramref name="expression" />.Type or its base types.</exception>
        public static MemberExpression Property(
          Expression expression,
          string propertyName)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return Expression.Property(expression, (expression.Type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy) ?? expression.Type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) ?? throw Error.PropertyNotDefinedForType((object)propertyName, (object)expression.Type));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing a property or field given the name of the property or field.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.MemberExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.MemberAccess" />, the <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property set to <paramref name="expression" />, and the <see cref="P:System.Linq.Expressions.MemberExpression.Member" /> property set to the <see cref="T:System.Reflection.PropertyInfo" /> or <see cref="T:System.Reflection.FieldInfo" /> that represents the property or field denoted by <paramref name="propertyOrFieldName" />.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> whose <see cref="P:System.Linq.Expressions.Expression.Type" /> contains a property or field named <paramref name="propertyOrFieldName" />.</param>
        /// <param name="propertyOrFieldName">The name of a property or field.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="propertyOrFieldName" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">No property or field named <paramref name="propertyOrFieldName" /> is defined in <paramref name="expression" />.Type or its base types.</exception>
        public static MemberExpression PropertyOrField(
          Expression expression,
          string propertyOrFieldName)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            PropertyInfo property1 = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (property1 != null)
                return Expression.Property(expression, property1);
            FieldInfo field = expression.Type.GetField(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (field != null)
                return Expression.Field(expression, field);
            PropertyInfo property2 = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (property2 != null)
                return Expression.Property(expression, property2);
            return Expression.Field(expression, expression.Type.GetField(propertyOrFieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy) ?? throw Error.NotAMemberOfType((object)propertyOrFieldName, (object)expression.Type));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents an expression that has a constant value of type <see cref="T:System.Linq.Expressions.Expression" />.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Quote" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property set to the specified value.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> is null.</exception>
        public static UnaryExpression Quote(Expression expression) => expression != null ? new UnaryExpression(ExpressionType.Quote, expression, expression.GetType()) : throw Error.ArgumentNull(nameof(expression));

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise right-shift operation.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.RightShift" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The right-shift operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression RightShift(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return Expression.IsInteger(left.Type) && Expression.GetNonNullableType(right.Type) == typeof(int) ? new BinaryExpression(ExpressionType.RightShift, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.RightShift, "op_RightShift", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents a bitwise right-shift operation. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.RightShift" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the right-shift operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression RightShift(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.RightShift(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.RightShift, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic subtraction operation that does not have overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Subtract" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The subtraction operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Subtract(Expression left, Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.Subtract, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.Subtract, "op_Subtraction", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic subtraction operation that does not have overflow checking. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Subtract" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the subtraction operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression Subtract(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.Subtract(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.Subtract, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic subtraction operation that has overflow checking.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.SubtractChecked" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> and <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The subtraction operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression SubtractChecked(
          Expression left,
          Expression right)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return left.Type == right.Type && Expression.IsArithmetic(left.Type) ? new BinaryExpression(ExpressionType.SubtractChecked, left, right, left.Type) : Expression.GetUserDefinedBinaryOperatorOrThrow(ExpressionType.SubtractChecked, "op_Subtraction", left, right, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" /> that represents an arithmetic subtraction operation that has overflow checking. The implementing method can be specified.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.BinaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.SubtractChecked" /> and the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" />, <see cref="P:System.Linq.Expressions.BinaryExpression.Right" />, and <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> properties set to the specified values.</returns>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Left" /> property equal to.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Right" /> property equal to.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> to set the <see cref="P:System.Linq.Expressions.BinaryExpression.Method" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="method" /> is not null and the method it represents returns void, is not static (Shared in Visual Basic), or does not take exactly two arguments.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="method" /> is null and the subtraction operator is not defined for <paramref name="left" />.Type and <paramref name="right" />.Type.</exception>
        public static BinaryExpression SubtractChecked(
          Expression left,
          Expression right,
          MethodInfo method)
        {
            if (left == null)
                throw Error.ArgumentNull(nameof(left));
            if (right == null)
                throw Error.ArgumentNull(nameof(right));
            return method == null ? Expression.SubtractChecked(left, right) : Expression.GetMethodBasedBinaryOperator(ExpressionType.SubtractChecked, left, right, method, true);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" /> that represents an explicit reference or boxing conversion where null is supplied if the conversion fails.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.TypeAs" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.Expression.Type" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.Expression.Type" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="type" /> is null.</exception>
        public static UnaryExpression TypeAs(Expression expression, Type type)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            return !type.IsValueType || Expression.IsNullableType(type) ? new UnaryExpression(ExpressionType.TypeAs, expression, type) : throw Error.IncorrectTypeForTypeAs((object)type);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.TypeBinaryExpression" />.</summary>
        /// <returns>A <see cref="T:System.Linq.Expressions.UnaryExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.TypeIs" /> and the <see cref="P:System.Linq.Expressions.TypeBinaryExpression.Expression" /> and <see cref="P:System.Linq.Expressions.TypeBinaryExpression.TypeOperand" /> properties set to the specified values.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.TypeBinaryExpression.Expression" /> property equal to.</param>
        /// <param name="type">A <see cref="T:System.Type" /> to set the <see cref="P:System.Linq.Expressions.TypeBinaryExpression.TypeOperand" /> property equal to.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="type" /> is null.</exception>
        public static TypeBinaryExpression TypeIs(Expression expression, Type type)
        {
            if (expression == null)
                throw Error.ArgumentNull(nameof(expression));
            return type != null ? new TypeBinaryExpression(ExpressionType.TypeIs, expression, type, typeof(bool)) : throw Error.ArgumentNull(nameof(type));
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" />, given an operand, by calling the appropriate factory method.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.UnaryExpression" /> that results from calling the appropriate factory method.</returns>
        /// <param name="unaryType">The <see cref="T:System.Linq.Expressions.ExpressionType" /> that specifies the type of unary operation.</param>
        /// <param name="operand">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the operand.</param>
        /// <param name="type">The <see cref="T:System.Type" /> that specifies the type to be converted to (pass null if not applicable).</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="unaryType" /> does not correspond to a unary expression node.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="operand" /> is null.</exception>
        public static UnaryExpression MakeUnary(
          ExpressionType unaryType,
          Expression operand,
          Type type)
        {
            return Expression.MakeUnary(unaryType, operand, type, (MethodInfo)null);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.UnaryExpression" />, given an operand and implementing method, by calling the appropriate factory method.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.UnaryExpression" /> that results from calling the appropriate factory method.</returns>
        /// <param name="unaryType">The <see cref="T:System.Linq.Expressions.ExpressionType" /> that specifies the type of unary operation.</param>
        /// <param name="operand">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the operand.</param>
        /// <param name="type">The <see cref="T:System.Type" /> that specifies the type to be converted to (pass null if not applicable).</param>
        /// <param name="method">The <see cref="T:System.Reflection.MethodInfo" /> that represents the implementing method.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="unaryType" /> does not correspond to a unary expression node.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="operand" /> is null.</exception>
        public static UnaryExpression MakeUnary(
          ExpressionType unaryType,
          Expression operand,
          Type type,
          MethodInfo method)
        {
            switch (unaryType)
            {
                case ExpressionType.ArrayLength:
                    return Expression.ArrayLength(operand);
                case ExpressionType.Convert:
                    return Expression.Convert(operand, type, method);
                case ExpressionType.ConvertChecked:
                    return Expression.ConvertChecked(operand, type, method);
                case ExpressionType.Negate:
                    return Expression.Negate(operand, method);
                case ExpressionType.NegateChecked:
                    return Expression.NegateChecked(operand, method);
                case ExpressionType.Not:
                    return Expression.Not(operand, method);
                case ExpressionType.Quote:
                    return Expression.Quote(operand);
                case ExpressionType.TypeAs:
                    return Expression.TypeAs(operand, type);
                default:
                    throw Error.UnhandledUnary((object)unaryType);
            }
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" />, given the left and right operands, by calling an appropriate factory method.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.BinaryExpression" /> that results from calling the appropriate factory method.</returns>
        /// <param name="binaryType">The <see cref="T:System.Linq.Expressions.ExpressionType" /> that specifies the type of binary operation.</param>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the left operand.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the right operand.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="binaryType" /> does not correspond to a binary expression node.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        public static BinaryExpression MakeBinary(
          ExpressionType binaryType,
          Expression left,
          Expression right)
        {
            return Expression.MakeBinary(binaryType, left, right, false, (MethodInfo)null);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" />, given the left operand, right operand and implementing method, by calling the appropriate factory method.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.BinaryExpression" /> that results from calling the appropriate factory method.</returns>
        /// <param name="binaryType">The <see cref="T:System.Linq.Expressions.ExpressionType" /> that specifies the type of binary operation.</param>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the left operand.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the right operand.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> that specifies the implementing method.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="binaryType" /> does not correspond to a binary expression node.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        public static BinaryExpression MakeBinary(
          ExpressionType binaryType,
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method)
        {
            return Expression.MakeBinary(binaryType, left, right, liftToNull, method, (LambdaExpression)null);
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.BinaryExpression" />, given the left operand, right operand, implementing method and type conversion function, by calling the appropriate factory method.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.BinaryExpression" /> that results from calling the appropriate factory method.</returns>
        /// <param name="binaryType">The <see cref="T:System.Linq.Expressions.ExpressionType" /> that specifies the type of binary operation.</param>
        /// <param name="left">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the left operand.</param>
        /// <param name="right">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the right operand.</param>
        /// <param name="liftToNull">true to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to true; false to set <see cref="P:System.Linq.Expressions.BinaryExpression.IsLiftedToNull" /> to false.</param>
        /// <param name="method">A <see cref="T:System.Reflection.MethodInfo" /> that specifies the implementing method.</param>
        /// <param name="conversion">A <see cref="T:System.Linq.Expressions.LambdaExpression" /> that represents a type conversion function. This parameter is used only if <paramref name="binaryType" /> is <see cref="F:System.Linq.Expressions.ExpressionType.Coalesce" />.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="binaryType" /> does not correspond to a binary expression node.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="left" /> or <paramref name="right" /> is null.</exception>
        public static BinaryExpression MakeBinary(
          ExpressionType binaryType,
          Expression left,
          Expression right,
          bool liftToNull,
          MethodInfo method,
          LambdaExpression conversion)
        {
            switch (binaryType)
            {
                case ExpressionType.Add:
                    return Expression.Add(left, right, method);
                case ExpressionType.AddChecked:
                    return Expression.AddChecked(left, right, method);
                case ExpressionType.And:
                    return Expression.And(left, right, method);
                case ExpressionType.AndAlso:
                    return Expression.AndAlso(left, right);
                case ExpressionType.ArrayIndex:
                    return Expression.ArrayIndex(left, right);
                case ExpressionType.Coalesce:
                    return Expression.Coalesce(left, right, conversion);
                case ExpressionType.Divide:
                    return Expression.Divide(left, right, method);
                case ExpressionType.Equal:
                    return Expression.Equal(left, right, liftToNull, method);
                case ExpressionType.ExclusiveOr:
                    return Expression.ExclusiveOr(left, right, method);
                case ExpressionType.GreaterThan:
                    return Expression.GreaterThan(left, right, liftToNull, method);
                case ExpressionType.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right, liftToNull, method);
                case ExpressionType.LeftShift:
                    return Expression.LeftShift(left, right, method);
                case ExpressionType.LessThan:
                    return Expression.LessThan(left, right, liftToNull, method);
                case ExpressionType.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right, liftToNull, method);
                case ExpressionType.Modulo:
                    return Expression.Modulo(left, right, method);
                case ExpressionType.Multiply:
                    return Expression.Multiply(left, right, method);
                case ExpressionType.MultiplyChecked:
                    return Expression.MultiplyChecked(left, right, method);
                case ExpressionType.NotEqual:
                    return Expression.NotEqual(left, right, liftToNull, method);
                case ExpressionType.Or:
                    return Expression.Or(left, right, method);
                case ExpressionType.OrElse:
                    return Expression.OrElse(left, right);
                case ExpressionType.Power:
                    return Expression.Power(left, right, method);
                case ExpressionType.RightShift:
                    return Expression.RightShift(left, right, method);
                case ExpressionType.Subtract:
                    return Expression.Subtract(left, right, method);
                case ExpressionType.SubtractChecked:
                    return Expression.SubtractChecked(left, right, method);
                default:
                    throw Error.UnhandledBinary((object)binaryType);
            }
        }

        /// <summary>Creates a <see cref="T:System.Linq.Expressions.MemberExpression" /> that represents accessing either a field or a property.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.MemberExpression" /> that results from calling the appropriate factory method.</returns>
        /// <param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> that represents the object that the member belongs to.</param>
        /// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> that describes the field or property to be accessed.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="expression" /> or <paramref name="member" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="member" /> does not represent a field or property.</exception>
        public static MemberExpression MakeMemberAccess(
          Expression expression,
          MemberInfo member)
        {
            switch (member)
            {
                case null:
                    throw Error.ArgumentNull(nameof(member));
                case FieldInfo field:
                    return Expression.Field(expression, field);
                case PropertyInfo property:
                    return Expression.Property(expression, property);
                default:
                    throw Error.MemberNotFieldOrProperty((object)member);
            }
        }

        private static BinaryExpression GetEqualityComparisonOperator(
          ExpressionType binaryType,
          string opName,
          Expression left,
          Expression right,
          bool liftToNull)
        {
            if (left.Type == right.Type && (Expression.IsNumeric(left.Type) || left.Type == typeof(object)))
                return Expression.IsNullableType(left.Type) && liftToNull ? new BinaryExpression(binaryType, left, right, typeof(bool?)) : new BinaryExpression(binaryType, left, right, typeof(bool));
            BinaryExpression definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(binaryType, opName, left, right, liftToNull);
            if (definedBinaryOperator != null)
                return definedBinaryOperator;
            if (!Expression.HasBuiltInEqualityOperator(left.Type, right.Type) && !Expression.IsNullComparison(left, right))
                throw Error.BinaryOperatorNotDefined((object)binaryType, (object)left.Type, (object)right.Type);
            return Expression.IsNullableType(left.Type) && liftToNull ? new BinaryExpression(binaryType, left, right, typeof(bool?)) : new BinaryExpression(binaryType, left, right, typeof(bool));
        }

        private static bool IsNullComparison(Expression left, Expression right)
        {
            if (Expression.IsNullConstant(left) && !Expression.IsNullConstant(right) && Expression.IsNullableType(right.Type))
                return true;
            return Expression.IsNullConstant(right) && !Expression.IsNullConstant(left) && Expression.IsNullableType(left.Type);
        }

        private static bool HasBuiltInEqualityOperator(Type left, Type right)
        {
            if (left.IsInterface && !right.IsValueType || right.IsInterface && !left.IsValueType || !left.IsValueType && !right.IsValueType && (Expression.AreReferenceAssignable(left, right) || Expression.AreReferenceAssignable(right, left)))
                return true;
            if (left != right)
                return false;
            Type nonNullableType = Expression.GetNonNullableType(left);
            return nonNullableType == typeof(bool) || Expression.IsNumeric(nonNullableType) || nonNullableType.IsEnum;
        }

        private static BinaryExpression GetComparisonOperator(
          ExpressionType binaryType,
          string opName,
          Expression left,
          Expression right,
          bool liftToNull)
        {
            if (left.Type != right.Type || !Expression.IsNumeric(left.Type))
                return Expression.GetUserDefinedBinaryOperatorOrThrow(binaryType, opName, left, right, liftToNull);
            return Expression.IsNullableType(left.Type) && liftToNull ? new BinaryExpression(binaryType, left, right, typeof(bool?)) : new BinaryExpression(binaryType, left, right, typeof(bool));
        }

        private static UnaryExpression GetUserDefinedCoercionOrThrow(
          ExpressionType coercionType,
          Expression expression,
          Type convertToType)
        {
            return Expression.GetUserDefinedCoercion(coercionType, expression, convertToType) ?? throw Error.CoercionOperatorNotDefined((object)expression.Type, (object)convertToType);
        }

        private static UnaryExpression GetUserDefinedCoercion(
          ExpressionType coercionType,
          Expression expression,
          Type convertToType)
        {
            Type nonNullableType1 = Expression.GetNonNullableType(expression.Type);
            Type nonNullableType2 = Expression.GetNonNullableType(convertToType);
            MethodInfo[] methods1 = nonNullableType1.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo conversionOperator1 = Expression.FindConversionOperator(methods1, expression.Type, convertToType);
            if (conversionOperator1 != null)
                return new UnaryExpression(coercionType, expression, conversionOperator1, convertToType);
            MethodInfo[] methods2 = nonNullableType2.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo conversionOperator2 = Expression.FindConversionOperator(methods2, expression.Type, convertToType);
            if (conversionOperator2 != null)
                return new UnaryExpression(coercionType, expression, conversionOperator2, convertToType);
            if (nonNullableType1 != expression.Type || nonNullableType2 != convertToType)
            {
                MethodInfo method = Expression.FindConversionOperator(methods1, nonNullableType1, nonNullableType2) ?? Expression.FindConversionOperator(methods2, nonNullableType1, nonNullableType2);
                if (method != null)
                    return new UnaryExpression(coercionType, expression, method, convertToType);
            }
            return (UnaryExpression)null;
        }

        private static MethodInfo FindConversionOperator(
          MethodInfo[] methods,
          Type typeFrom,
          Type typeTo)
        {
            foreach (MethodInfo method in methods)
            {
                if ((!(method.Name != "op_Implicit") || !(method.Name != "op_Explicit")) && method.ReturnType == typeTo && method.GetParameters()[0].ParameterType == typeFrom)
                    return method;
            }
            return (MethodInfo)null;
        }

        private static UnaryExpression GetUserDefinedUnaryOperatorOrThrow(
          ExpressionType unaryType,
          string name,
          Expression operand)
        {
            UnaryExpression definedUnaryOperator = Expression.GetUserDefinedUnaryOperator(unaryType, name, operand);
            if (definedUnaryOperator == null)
                throw Error.UnaryOperatorNotDefined((object)unaryType, (object)operand.Type);
            Expression.ValidateParamswithOperandsOrThrow(definedUnaryOperator.Method.GetParameters()[0].ParameterType, operand.Type, unaryType, name);
            return definedUnaryOperator;
        }

        private static UnaryExpression GetUserDefinedUnaryOperator(
          ExpressionType unaryType,
          string name,
          Expression operand)
        {
            Type type = operand.Type;
            Type[] types = new Type[1] { type };
            Type nonNullableType = Expression.GetNonNullableType(type);
            MethodInfo method1 = nonNullableType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types, (ParameterModifier[])null);
            if (method1 != null)
                return new UnaryExpression(unaryType, operand, method1, method1.ReturnType);
            if (Expression.IsNullableType(type))
            {
                types[0] = nonNullableType;
                MethodInfo method2 = nonNullableType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types, (ParameterModifier[])null);
                if (method2 != null && method2.ReturnType.IsValueType && !Expression.IsNullableType(method2.ReturnType))
                    return new UnaryExpression(unaryType, operand, method2, Expression.GetNullableType(method2.ReturnType));
            }
            return (UnaryExpression)null;
        }

        private static void ValidateParamswithOperandsOrThrow(
          Type paramType,
          Type operandType,
          ExpressionType exprType,
          string name)
        {
            if (Expression.IsNullableType(paramType) && !Expression.IsNullableType(operandType))
                throw Error.OperandTypesDoNotMatchParameters((object)exprType, (object)name);
        }

        private static BinaryExpression GetUserDefinedBinaryOperatorOrThrow(
          ExpressionType binaryType,
          string name,
          Expression left,
          Expression right,
          bool liftToNull)
        {
            BinaryExpression definedBinaryOperator = Expression.GetUserDefinedBinaryOperator(binaryType, name, left, right, liftToNull);
            if (definedBinaryOperator == null)
                throw Error.BinaryOperatorNotDefined((object)binaryType, (object)left.Type, (object)right.Type);
            Expression.ValidateParamswithOperandsOrThrow(definedBinaryOperator.Method.GetParameters()[0].ParameterType, left.Type, binaryType, name);
            Expression.ValidateParamswithOperandsOrThrow(definedBinaryOperator.Method.GetParameters()[1].ParameterType, right.Type, binaryType, name);
            return definedBinaryOperator;
        }

        private static MethodInfo GetUserDefinedBinaryOperator(
          ExpressionType binaryType,
          Type leftType,
          Type rightType,
          string name)
        {
            Type[] types = new Type[2] { leftType, rightType };
            Type nonNullableType1 = Expression.GetNonNullableType(leftType);
            Type nonNullableType2 = Expression.GetNonNullableType(rightType);
            BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo method = nonNullableType1.GetMethod(name, bindingAttr, (Binder)null, types, (ParameterModifier[])null) ?? nonNullableType2.GetMethod(name, bindingAttr, (Binder)null, types, (ParameterModifier[])null);
            if (Expression.IsLiftingConditionalLogicalOperator(leftType, rightType, method, binaryType))
                method = Expression.GetUserDefinedBinaryOperator(binaryType, nonNullableType1, nonNullableType2, name);
            return method;
        }

        private static bool IsLiftingConditionalLogicalOperator(
          Type left,
          Type right,
          MethodInfo method,
          ExpressionType binaryType)
        {
            if (!Expression.IsNullableType(right) || !Expression.IsNullableType(left) || method != null)
                return false;
            return binaryType == ExpressionType.AndAlso || binaryType == ExpressionType.OrElse;
        }

        private static BinaryExpression GetUserDefinedBinaryOperator(
          ExpressionType binaryType,
          string name,
          Expression left,
          Expression right,
          bool liftToNull)
        {
            MethodInfo definedBinaryOperator1 = Expression.GetUserDefinedBinaryOperator(binaryType, left.Type, right.Type, name);
            if (definedBinaryOperator1 != null)
                return new BinaryExpression(binaryType, left, right, definedBinaryOperator1, definedBinaryOperator1.ReturnType);
            if (Expression.IsNullableType(left.Type) && Expression.IsNullableType(right.Type))
            {
                Type nonNullableType1 = Expression.GetNonNullableType(left.Type);
                Type nonNullableType2 = Expression.GetNonNullableType(right.Type);
                MethodInfo definedBinaryOperator2 = Expression.GetUserDefinedBinaryOperator(binaryType, nonNullableType1, nonNullableType2, name);
                if (definedBinaryOperator2 != null && definedBinaryOperator2.ReturnType.IsValueType && !Expression.IsNullableType(definedBinaryOperator2.ReturnType))
                    return definedBinaryOperator2.ReturnType != typeof(bool) || liftToNull ? new BinaryExpression(binaryType, left, right, definedBinaryOperator2, Expression.GetNullableType(definedBinaryOperator2.ReturnType)) : new BinaryExpression(binaryType, left, right, definedBinaryOperator2, typeof(bool));
            }
            return (BinaryExpression)null;
        }

        private static void ValidateOperator(MethodInfo method)
        {
            Expression.ValidateMethodInfo(method);
            if (!method.IsStatic)
                throw Error.UserDefinedOperatorMustBeStatic((object)method);
            if (method.ReturnType == typeof(void))
                throw Error.UserDefinedOperatorMustNotBeVoid((object)method);
        }

        private static void ValidateUserDefinedConditionalLogicOperator(
          ExpressionType nodeType,
          Type left,
          Type right,
          MethodInfo method)
        {
            Expression.ValidateOperator(method);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 2)
                throw Error.IncorrectNumberOfMethodCallArguments((object)method);
            if (!Expression.ParameterIsAssignable(parameters[0], left) && (!Expression.IsNullableType(left) || !Expression.ParameterIsAssignable(parameters[0], Expression.GetNonNullableType(left))))
                throw Error.OperandTypesDoNotMatchParameters((object)nodeType, (object)method.Name);
            if (!Expression.ParameterIsAssignable(parameters[1], right) && (!Expression.IsNullableType(right) || !Expression.ParameterIsAssignable(parameters[1], Expression.GetNonNullableType(right))))
                throw Error.OperandTypesDoNotMatchParameters((object)nodeType, (object)method.Name);
            if (parameters[0].ParameterType != parameters[1].ParameterType)
                throw Error.LogicalOperatorMustHaveConsistentTypes((object)nodeType, (object)method.Name);
            if (method.ReturnType != parameters[0].ParameterType)
                throw Error.LogicalOperatorMustHaveConsistentTypes((object)nodeType, (object)method.Name);
            if (Expression.IsValidLiftedConditionalLogicalOperator(left, right, parameters))
            {
                left = Expression.GetNonNullableType(left);
                right = Expression.GetNonNullableType(left);
            }
            Type[] types = new Type[1]
            {
        parameters[0].ParameterType
            };
            MethodInfo method1 = method.DeclaringType.GetMethod("op_True", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types, (ParameterModifier[])null);
            MethodInfo method2 = method.DeclaringType.GetMethod("op_False", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types, (ParameterModifier[])null);
            if (method1 == null || method2 == null)
                throw Error.LogicalOperatorMustHaveBooleanOperators((object)nodeType, (object)method.Name);
            if (method1.ReturnType != typeof(bool))
                throw Error.LogicalOperatorMustHaveBooleanOperators((object)nodeType, (object)method.Name);
            if (method2.ReturnType != typeof(bool))
                throw Error.LogicalOperatorMustHaveBooleanOperators((object)nodeType, (object)method.Name);
        }

        private static bool IsValidLiftedConditionalLogicalOperator(
          Type left,
          Type right,
          ParameterInfo[] pms)
        {
            return left == right && Expression.IsNullableType(right) && pms[1].ParameterType == Expression.GetNonNullableType(right);
        }

        private static UnaryExpression GetMethodBasedCoercionOperator(
          ExpressionType unaryType,
          Expression operand,
          Type convertToType,
          MethodInfo method)
        {
            Expression.ValidateOperator(method);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments((object)method);
            if (Expression.ParameterIsAssignable(parameters[0], operand.Type) && method.ReturnType == convertToType)
                return new UnaryExpression(unaryType, operand, method, method.ReturnType);
            if ((Expression.IsNullableType(operand.Type) || Expression.IsNullableType(convertToType)) && Expression.ParameterIsAssignable(parameters[0], Expression.GetNonNullableType(operand.Type)) && method.ReturnType == Expression.GetNonNullableType(convertToType))
                return new UnaryExpression(unaryType, operand, method, convertToType);
            throw Error.OperandTypesDoNotMatchParameters((object)unaryType, (object)method.Name);
        }

        private static UnaryExpression GetMethodBasedUnaryOperator(
          ExpressionType unaryType,
          Expression operand,
          MethodInfo method)
        {
            Expression.ValidateOperator(method);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1)
                throw Error.IncorrectNumberOfMethodCallArguments((object)method);
            if (Expression.ParameterIsAssignable(parameters[0], operand.Type))
            {
                Expression.ValidateParamswithOperandsOrThrow(parameters[0].ParameterType, operand.Type, unaryType, method.Name);
                return new UnaryExpression(unaryType, operand, method, method.ReturnType);
            }
            if (Expression.IsNullableType(operand.Type) && Expression.ParameterIsAssignable(parameters[0], Expression.GetNonNullableType(operand.Type)) && method.ReturnType.IsValueType && !Expression.IsNullableType(method.ReturnType))
                return new UnaryExpression(unaryType, operand, method, Expression.GetNullableType(method.ReturnType));
            throw Error.OperandTypesDoNotMatchParameters((object)unaryType, (object)method.Name);
        }

        private static BinaryExpression GetMethodBasedBinaryOperator(
          ExpressionType binaryType,
          Expression left,
          Expression right,
          MethodInfo method,
          bool liftToNull)
        {
            Expression.ValidateOperator(method);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 2)
                throw Error.IncorrectNumberOfMethodCallArguments((object)method);
            if (Expression.ParameterIsAssignable(parameters[0], left.Type) && Expression.ParameterIsAssignable(parameters[1], right.Type))
            {
                Expression.ValidateParamswithOperandsOrThrow(parameters[0].ParameterType, left.Type, binaryType, method.Name);
                Expression.ValidateParamswithOperandsOrThrow(parameters[1].ParameterType, right.Type, binaryType, method.Name);
                return new BinaryExpression(binaryType, left, right, method, method.ReturnType);
            }
            if (!Expression.IsNullableType(left.Type) || !Expression.IsNullableType(right.Type) || !Expression.ParameterIsAssignable(parameters[0], Expression.GetNonNullableType(left.Type)) || !Expression.ParameterIsAssignable(parameters[1], Expression.GetNonNullableType(right.Type)) || !method.ReturnType.IsValueType || Expression.IsNullableType(method.ReturnType))
                throw Error.OperandTypesDoNotMatchParameters((object)binaryType, (object)method.Name);
            return method.ReturnType != typeof(bool) || liftToNull ? new BinaryExpression(binaryType, left, right, method, Expression.GetNullableType(method.ReturnType)) : new BinaryExpression(binaryType, left, right, method, typeof(bool));
        }

        private static bool ParameterIsAssignable(ParameterInfo pi, Type argType)
        {
            Type dest = pi.ParameterType;
            if (dest.IsByRef)
                dest = dest.GetElementType();
            return Expression.AreReferenceAssignable(dest, argType);
        }

        private static void ValidateIntegerArg(Type type)
        {
            if (!Expression.IsInteger(type))
                throw Error.ArgumentMustBeInteger();
        }

        private static void ValidateIntegerOrBoolArg(Type type)
        {
            if (!Expression.IsIntegerOrBool(type))
                throw Error.ArgumentMustBeIntegerOrBoolean();
        }

        private static void ValidateNumericArg(Type type)
        {
            if (!Expression.IsNumeric(type))
                throw Error.ArgumentMustBeNumeric();
        }

        private static void ValidateConvertibleArg(Type type)
        {
            if (!Expression.IsConvertible(type))
                throw Error.ArgumentMustBeConvertible();
        }

        private static void ValidateBoolArg(Type type)
        {
            if (!Expression.IsBool(type))
                throw Error.ArgumentMustBeBoolean();
        }

        private static Type ValidateCoalesceArgTypes(Type left, Type right)
        {
            Type nonNullableType = Expression.GetNonNullableType(left);
            if (left.IsValueType && !Expression.IsNullableType(left))
                throw Error.CoalesceUsedOnNonNullType();
            if (Expression.IsNullableType(left) && Expression.IsImplicitlyConvertible(right, nonNullableType))
                return nonNullableType;
            if (Expression.IsImplicitlyConvertible(right, left))
                return left;
            return Expression.IsImplicitlyConvertible(nonNullableType, right) ? right : throw Error.ArgumentTypesMustMatch();
        }

        private static void ValidateSameArgTypes(Type left, Type right)
        {
            if (left != right)
                throw Error.ArgumentTypesMustMatch();
        }

        private static void ValidateElementInitAddMethodInfo(MethodInfo addMethod)
        {
            Expression.ValidateMethodInfo(addMethod);
            if (addMethod.GetParameters().Length == 0)
                throw Error.ElementInitializerMethodWithZeroArgs();
            if (!addMethod.Name.Equals("Add", StringComparison.OrdinalIgnoreCase))
                throw Error.ElementInitializerMethodNotAdd();
            if (addMethod.IsStatic)
                throw Error.ElementInitializerMethodStatic();
            foreach (ParameterInfo parameter in addMethod.GetParameters())
            {
                if (parameter.ParameterType.IsByRef)
                    throw Error.ElementInitializerMethodNoRefOutParam((object)parameter.Name, (object)addMethod.Name);
            }
        }

        private static void ValidateMethodInfo(MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
                throw Error.MethodIsGeneric((object)method);
            if (method.ContainsGenericParameters)
                throw Error.MethodContainsGenericParameters((object)method);
        }

        private static void ValidateType(Type type)
        {
            if (type.IsGenericTypeDefinition)
                throw Error.TypeIsGeneric((object)type);
            if (type.ContainsGenericParameters)
                throw Error.TypeContainsGenericParameters((object)type);
        }

        internal static Type GetNullableType(Type type)
        {
            if (type == null)
                throw Error.ArgumentNull(nameof(type));
            if (!type.IsValueType || Expression.IsNullableType(type))
                return type;
            return typeof(Nullable<>).MakeGenericType(type);
        }

        private static bool IsSameOrSubclass(Type type, Type subType) => type == subType || subType.IsSubclassOf(type);

        private static bool AreReferenceAssignable(Type dest, Type src) => dest == src || !dest.IsValueType && !src.IsValueType && Expression.AreAssignable(dest, src);

        private static bool AreAssignable(Type dest, Type src) => dest == src || dest.IsAssignableFrom(src) || dest.IsArray && src.IsArray && dest.GetArrayRank() == src.GetArrayRank() && Expression.AreReferenceAssignable(dest.GetElementType(), src.GetElementType()) || src.IsArray && dest.IsGenericType && (dest.GetGenericTypeDefinition() == typeof(IEnumerable<>) || dest.GetGenericTypeDefinition() == typeof(IList<>) || dest.GetGenericTypeDefinition() == typeof(ICollection<>)) && dest.GetGenericArguments()[0] == src.GetElementType();

        internal static bool IsNullableType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        internal static Type GetNonNullableType(Type type)
        {
            if (Expression.IsNullableType(type))
                type = type.GetGenericArguments()[0];
            return type;
        }

        private static bool IsNullConstant(Expression expr) => expr is ConstantExpression constantExpression && constantExpression.Value == null;

        private static bool IsUnSigned(Type type)
        {
            type = Expression.GetNonNullableType(type);
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsArithmetic(Type type)
        {
            type = Expression.GetNonNullableType(type);
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsNumeric(Type type)
        {
            type = Expression.GetNonNullableType(type);
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsImplicitlyConvertible(Type source, Type destination) => Expression.IsIdentityConversion(source, destination) || Expression.IsImplicitNumericConversion(source, destination) || Expression.IsImplicitReferenceConversion(source, destination) || Expression.IsImplicitBoxingConversion(source, destination) || Expression.IsImplicitNullableConversion(source, destination);

        private static bool IsIdentityConversion(Type source, Type destination) => source == destination;

        private static bool IsImplicitNumericConversion(Type source, Type destination)
        {
            TypeCode typeCode1 = Type.GetTypeCode(source);
            TypeCode typeCode2 = Type.GetTypeCode(destination);
            switch (typeCode1)
            {
                case TypeCode.Char:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.SByte:
                    switch (typeCode2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Byte:
                    switch (typeCode2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int16:
                    switch (typeCode2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.UInt16:
                    switch (typeCode2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int32:
                    switch (typeCode2)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.UInt32:
                    switch (typeCode2)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (typeCode2)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Single:
                    return typeCode2 == TypeCode.Double;
                default:
                    return false;
            }
        }

        private static bool IsImplicitReferenceConversion(Type source, Type destination) => Expression.AreAssignable(destination, source);

        private static bool IsImplicitBoxingConversion(Type source, Type destination) => source.IsValueType && (destination == typeof(object) || destination == typeof(ValueType)) || source.IsEnum && destination == typeof(Enum);

        private static bool IsImplicitNullableConversion(Type source, Type destination) => Expression.IsNullableType(destination) && Expression.IsImplicitlyConvertible(Expression.GetNonNullableType(source), Expression.GetNonNullableType(destination));

        private static bool IsConvertible(Type type)
        {
            type = Expression.GetNonNullableType(type);
            if (type.IsEnum)
                return true;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsInteger(Type type)
        {
            type = Expression.GetNonNullableType(type);
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsIntegerOrBool(Type type)
        {
            type = Expression.GetNonNullableType(type);
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsBool(Type type)
        {
            type = Expression.GetNonNullableType(type);
            return type == typeof(bool);
        }

        internal static ReadOnlyCollection<T> ReturnReadOnly<T>(ref IList<T> collection)
        {
            IList<T> ts = collection;
            ReadOnlyCollection<T> ts1 = ts as ReadOnlyCollection<T>;
            if (ts1 != null)
            {
                return ts1;
            }
            System.Threading.Net20Interlocked.CompareExchange<IList<T>>(ref collection, ts.ToReadOnly<T>(), ts);
            return (ReadOnlyCollection<T>)collection;
        }

        internal class IndexExpressionProxy
        {
            private readonly IndexExpression _node;

            public ReadOnlyCollection<Expression> Arguments
            {
                get
                {
                    return this._node.Arguments;
                }
            }

            public bool CanReduce
            {
                get
                {
                    return this._node.CanReduce;
                }
            }

            public string DebugView
            {
                get
                {
                    return this._node.DebugView;
                }
            }

            public PropertyInfo Indexer
            {
                get
                {
                    return this._node.Indexer;
                }
            }

            public ExpressionType NodeType
            {
                get
                {
                    return this._node.NodeType;
                }
            }

            public Expression Object
            {
                get
                {
                    return this._node.Object;
                }
            }

            public Type Type
            {
                get
                {
                    return this._node.Type;
                }
            }

            public IndexExpressionProxy(IndexExpression node)
            {
                this._node = node;
            }
        }

    }

}

