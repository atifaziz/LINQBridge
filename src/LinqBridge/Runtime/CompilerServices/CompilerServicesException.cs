using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.CompilerServices
{
    public class CompilerServicesException: Exception
    {
        public CompilerServicesException()
        {
        }

        public CompilerServicesException(string message) : base(message)
        {
        }

        public CompilerServicesException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
