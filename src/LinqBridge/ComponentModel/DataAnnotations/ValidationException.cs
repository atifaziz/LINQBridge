using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel.DataAnnotations
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationAttribute ValidationAttribute
        {
            get;
            private set;
        }

        public object Value
        {
            get;
            private set;
        }

        public ValidationException(string errorMessage, ValidationAttribute validatingAttribute, object value) : base(errorMessage)
        {
            this.Value = value;
            this.ValidationAttribute = validatingAttribute;
        }

        public ValidationException()
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}