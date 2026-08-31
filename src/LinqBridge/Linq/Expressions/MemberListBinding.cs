using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents initializing the elements of a collection member of a newly created object.</summary>
    public sealed class MemberListBinding : MemberBinding
    {
        private ReadOnlyCollection<ElementInit> initializers;

        internal MemberListBinding(MemberInfo member, ReadOnlyCollection<ElementInit> initializers)
            : base(MemberBindingType.ListBinding, member)
        {
            this.initializers = initializers;
        }

        /// <summary>Gets the element initializers for initializing a collection member of a newly created object.</summary>
        /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.ElementInit" /> objects to initialize a collection member with.</returns>
        public ReadOnlyCollection<ElementInit> Initializers => this.initializers;

        internal override void BuildString(StringBuilder builder)
        {
            builder.Append(this.Member.Name);
            builder.Append(" = {");
            int index = 0;
            for (int count = this.initializers.Count; index < count; ++index)
            {
                if (index > 0)
                    builder.Append(", ");
                this.initializers[index].BuildString(builder);
            }
            builder.Append("}");
        }
    }
}