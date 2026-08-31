namespace System.Runtime.CompilerServices
{
    /// <summary>Defines a property for accessing the value that an object references.</summary>
    /// <filterpriority>2</filterpriority>
    public interface IStrongBox
    {
        /// <summary>Gets or sets the value that an object references.</summary>
        /// <returns>The value that the object references.</returns>
        object Value { get; set; }
    }
}