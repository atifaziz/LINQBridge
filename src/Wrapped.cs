namespace LinqBridge
{

    internal struct Wrapped<TKey>
    {
        public Wrapped(TKey value)
        {
            m_Value = value;
        }

        private readonly TKey m_Value;

        public TKey Value
        {
            get
            {
                return m_Value;
            }
        }
    }
}
