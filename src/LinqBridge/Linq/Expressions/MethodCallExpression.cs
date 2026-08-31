using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents calling a method.</summary>
    public sealed class MethodCallExpression : Expression
    {
        private MethodInfo method;
        private Expression obj;
        private ReadOnlyCollection<Expression> arguments;

        internal MethodCallExpression(
          ExpressionType type,
          MethodInfo method,
          Expression obj,
          ReadOnlyCollection<Expression> arguments)
          : base(type, method.ReturnType)
        {
            this.obj = obj;
            this.method = method;
            this.arguments = arguments;
        }

        /// <summary>Gets the called method.</summary>
        /// <returns>The <see cref="T:System.Reflection.MethodInfo" /> that represents the called method.</returns>
        public MethodInfo Method => this.method;

        /// <summary>Gets the receiving object of the method.</summary>
        /// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the receiving object of the method.</returns>
        public Expression Object => this.obj;

        /// <summary>Gets the arguments to the called method.</summary>
        /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.Expression" /> objects which represent the arguments to the called method.</returns>
        public ReadOnlyCollection<Expression> Arguments => this.arguments;

        internal override void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            int num = 0;
            Expression expression = this.obj;
            if (Attribute.GetCustomAttribute((MemberInfo)this.method, typeof(ExtensionAttribute)) != null)
            {
                num = 1;
                expression = this.arguments[0];
            }
            if (expression != null)
            {
                expression.BuildString(builder);
                builder.Append(".");
            }
            builder.Append(this.method.Name);
            builder.Append("(");
            int index = num;
            for (int count = this.arguments.Count; index < count; ++index)
            {
                if (index > num)
                    builder.Append(", ");
                this.arguments[index].BuildString(builder);
            }
            builder.Append(")");
        }
    }
}
