using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Provides the base class from which the classes that represent bindings that are used to initialize members of a newly created object derive.</summary>
    public abstract class MemberBinding
    {
        private MemberBindingType type;
        private MemberInfo member;

        /// <summary>Initializes a new instance of the <see cref="T:System.Linq.Expressions.MemberBinding" /> class.</summary>
        /// <param name="type">The <see cref="T:System.Linq.Expressions.MemberBindingType" /> that discriminates the type of binding that is represented.</param>
        /// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> that represents a field or property to be initialized.</param>
        protected MemberBinding(MemberBindingType type, MemberInfo member)
        {
            this.type = type;
            this.member = member;
        }

        /// <summary>Gets the type of binding that is represented.</summary>
        /// <returns>One of the <see cref="T:System.Linq.Expressions.MemberBindingType" /> values.</returns>
        public MemberBindingType BindingType => this.type;

        /// <summary>Gets the field or property to be initialized.</summary>
        /// <returns>The <see cref="T:System.Reflection.MemberInfo" /> that represents the field or property to be initialized.</returns>
        public MemberInfo Member => this.member;

        internal abstract void BuildString(StringBuilder builder);

        /// <summary>Returns a textual representation of the <see cref="T:System.Linq.Expressions.MemberBinding" />.</summary>
        /// <returns>A textual representation of the <see cref="T:System.Linq.Expressions.MemberBinding" />.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.BuildString(builder);
            return builder.ToString();
        }
    }
}
