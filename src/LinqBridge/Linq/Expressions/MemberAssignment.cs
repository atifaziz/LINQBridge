using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents initializing a field or property of a newly created object.</summary>
    public sealed class MemberAssignment : MemberBinding
    {
        private Expression expression;

        internal MemberAssignment(MemberInfo member, Expression expression)
            : base(MemberBindingType.Assignment, member)
        {
            this.expression = expression;
        }

        /// <summary>Gets the expression to assign to the field or property.</summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that represents the value to assign to the field or property.</returns>
        public Expression Expression => this.expression;

        internal override void BuildString(StringBuilder builder)
        {
            if (builder == null)
                throw Error.ArgumentNull(nameof(builder));
            builder.Append(this.Member.Name);
            builder.Append(" = ");
            this.expression.BuildString(builder);
        }
    }
}