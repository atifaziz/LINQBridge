namespace System.Linq.Expressions
{
    /// <summary>Describes the binding types that are used in <see cref="T:System.Linq.Expressions.MemberInitExpression" /> objects.</summary>
    public enum MemberBindingType
    {
        /// <summary>A binding that represents initializing a member with the value of an expression.</summary>
        Assignment,
        /// <summary>A binding that represents recursively initializing members of a member.</summary>
        MemberBinding,
        /// <summary>A binding that represents initializing a member of type <see cref="T:System.Collections.IList" /> or <see cref="T:System.Collections.Generic.ICollection`1" /> from a list of elements.</summary>
        ListBinding,
    }
}