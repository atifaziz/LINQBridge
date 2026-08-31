using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    /// <summary>Represents a constructor call.</summary>
    public sealed class NewExpression : Expression
    {
        private ConstructorInfo constructor;
        private ReadOnlyCollection<Expression> arguments;
        private ReadOnlyCollection<MemberInfo> members;

        internal NewExpression(
          Type type,
          ConstructorInfo constructor,
          ReadOnlyCollection<Expression> arguments)
          : base(ExpressionType.New, type)
        {
            this.constructor = constructor;
            this.arguments = arguments;
        }

        internal NewExpression(
          Type type,
          ConstructorInfo constructor,
          ReadOnlyCollection<Expression> arguments,
          ReadOnlyCollection<MemberInfo> members)
          : base(ExpressionType.New, type)
        {
            this.constructor = constructor;
            this.arguments = arguments;
            this.members = members;
        }

        /// <summary>Gets the called constructor.</summary>
        /// <returns>The <see cref="T:System.Reflection.ConstructorInfo" /> that represents the called constructor.</returns>
        public ConstructorInfo Constructor => this.constructor;

        /// <summary>Gets the arguments to the constructor.</summary>
        /// <returns>A collection of <see cref="T:System.Linq.Expressions.Expression" /> objects that represent the arguments to the constructor.</returns>
        public ReadOnlyCollection<Expression> Arguments => this.arguments;

        /// <summary>Gets the members that can retrieve the values of the fields that were initialized with constructor arguments.</summary>
        /// <returns>A collection of <see cref="T:System.Reflection.MemberInfo" /> objects that represent the members that can retrieve the values of the fields that were initialized with constructor arguments.</returns>
        public ReadOnlyCollection<MemberInfo> Members => this.members;

        private static PropertyInfo GetPropertyNoThrow(MethodInfo method)
        {
            if (method == null)
                return (PropertyInfo)null;
            foreach (PropertyInfo property in method.DeclaringType.GetProperties((BindingFlags)(48 | (method.IsStatic ? 8 : 4))))
            {
                if (property.CanRead && method == property.GetGetMethod(true) || property.CanWrite && method == property.GetSetMethod(true))
                    return property;
            }
            return (PropertyInfo)null;
        }

        internal override void BuildString(StringBuilder builder)
        {
            Type type1;
            if (this.constructor != null)
            {
                type1 = this.constructor.DeclaringType;
            }
            else
            {
                Type type2 = type1 = this.Type;
            }
            Type type3 = type1;
            builder.Append("new ");
            int count = this.arguments.Count;
            builder.Append(type3.Name);
            builder.Append("(");
            if (count > 0)
            {
                for (int index = 0; index < count; ++index)
                {
                    if (index > 0)
                        builder.Append(", ");
                    if (this.members != null)
                    {
                        PropertyInfo propertyNoThrow;
                        if (this.members[index].MemberType == MemberTypes.Method && (propertyNoThrow = NewExpression.GetPropertyNoThrow((MethodInfo)this.members[index])) != null)
                            builder.Append(propertyNoThrow.Name);
                        else
                            builder.Append(this.members[index].Name);
                        builder.Append(" = ");
                    }
                    this.arguments[index].BuildString(builder);
                }
            }
            builder.Append(")");
        }
    }
}
