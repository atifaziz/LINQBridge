using System;
using System.ComponentModel.DataAnnotations.Resources;
using System.Runtime.CompilerServices;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredAttribute : ValidationAttribute
    {
        public RequiredAttribute() : base(() => DataAnnotationsResources.RequiredAttribute_ValidationError)
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }
            string str = value as string;
            if (str == null)
            {
                return true;
            }
            return str.Trim().Length != 0;
        }
    }
}