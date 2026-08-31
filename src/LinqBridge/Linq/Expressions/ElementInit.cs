using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents an initializer for a single element of an <see cref="T:System.Collections.IEnumerable" /> collection.</summary>
    public sealed class ElementInit
    {
        private MethodInfo addMethod;
        private ReadOnlyCollection<Expression> arguments;

        internal ElementInit(MethodInfo addMethod, ReadOnlyCollection<Expression> arguments)
        {
            this.addMethod = addMethod;
            this.arguments = arguments;
        }

        /// <summary>Gets the instance method that is used to add an element to an <see cref="T:System.Collections.IEnumerable" /> collection.</summary>
        /// <returns>A <see cref="T:System.Reflection.MethodInfo" /> that represents an instance method that adds an element to a collection.</returns>
        public MethodInfo AddMethod => this.addMethod;

        /// <summary>Gets the collection of arguments that are passed to a method that adds an element to an <see cref="T:System.Collections.IEnumerable" /> collection.</summary>
        /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.Expression" /> objects that represent the arguments for a method that adds an element to a collection.</returns>
        public ReadOnlyCollection<Expression> Arguments => this.arguments;

        internal void BuildString(StringBuilder builder)
        {
            builder.Append((object)this.AddMethod);
            builder.Append("(");
            bool flag = true;
            foreach (Expression expression in this.arguments)
            {
                if (flag)
                    flag = false;
                else
                    builder.Append(",");
                expression.BuildString(builder);
            }
            builder.Append(")");
        }

        /// <summary>Returns a textual representation of an <see cref="T:System.Linq.Expressions.ElementInit" /> object.</summary>
        /// <returns>A textual representation of the <see cref="T:System.Linq.Expressions.ElementInit" /> object.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.BuildString(builder);
            return builder.ToString();
        }
    }
}
