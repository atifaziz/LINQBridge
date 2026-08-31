namespace System.Runtime.CompilerServices
{
    /// <summary>Holds a reference to a value.</summary>
    /// <typeparam name="T">The type of the value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</typeparam>
    /// <filterpriority>2</filterpriority>
    public class StrongBox<T> : IStrongBox
    {
        /// <summary>Represents the value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</summary>
        public T Value;

        /// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> class by using the supplied value. </summary>
        /// <param name="value">A value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> will reference.</param>
        /// <filterpriority>2</filterpriority>
        public StrongBox(T value) => this.Value = value;

        /// <summary>Gets or sets the value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</summary>
        /// <returns>The value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</returns>
        object IStrongBox.Value
        {
            get => (object)this.Value;
            set => this.Value = (T)value;
        }
    }
}