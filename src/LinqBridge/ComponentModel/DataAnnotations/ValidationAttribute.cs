// Decompiled with JetBrains decompiler
// Type: System.ComponentModel.DataAnnotations.ValidationAttribute
// Assembly: System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: 3C95AFC2-1C2E-4E83-9200-C84F8A48B546
// Assembly location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.ComponentModel.DataAnnotations.dll
// XML documentation location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.ComponentModel.DataAnnotations.xml

using System.ComponentModel.DataAnnotations.Resources;
using System.Globalization;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>Serves as the base class for all validation attributes.</summary>
    public abstract class ValidationAttribute : Attribute
    {
        private string _errorMessage;
        private string _errorMessageResourceName;
        private Type _errorMessageResourceType;
        private bool _resourceModeAccessorIncomplete;

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationAttribute" /> class.</summary>
        protected ValidationAttribute()
          : this((Func<string>)(() => DataAnnotationsResources.ValidationAttribute_ValidationError))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationAttribute" /> class by using the error message to associate with a validation control.</summary>
        /// <param name="errorMessage">The error message to associate with a validation control.</param>
        protected ValidationAttribute(string errorMessage)
          : this((Func<string>)(() => errorMessage))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationAttribute" /> class by using the function that enables access to validation resources.</summary>
        /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="errorMessageAccessor" /> is <see langword="null" />.</exception>
        protected ValidationAttribute(Func<string> errorMessageAccessor) => this.ResourceAccessor = errorMessageAccessor != null ? errorMessageAccessor : throw new ArgumentNullException(nameof(errorMessageAccessor));

        private Func<string> ResourceAccessor { get; set; }

        /// <summary>Gets the localized validation error message.</summary>
        /// <returns>The localized validation error message.</returns>
        protected string ErrorMessageString
        {
            get
            {
                if (this._resourceModeAccessorIncomplete)
                    throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, DataAnnotationsResources.ValidationAttribute_NeedBothResourceTypeAndResourceName));
                return this.ResourceAccessor();
            }
        }

        /// <summary>Gets or sets the resource type to use for error-message lookup if validation fails.</summary>
        /// <returns>The type of error message that is associated with a validation control.</returns>
        public Type ErrorMessageResourceType
        {
            get => this._errorMessageResourceType;
            set
            {
                if (this._errorMessage != null)
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_AlreadyInExplicitMode);
                if (this._errorMessageResourceType != null)
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_PropertyCannotBeSetMoreThanOnce);
                this._errorMessageResourceType = value != null ? value : throw new ArgumentNullException(nameof(value));
                this.SetResourceAccessorByPropertyLookup();
            }
        }

        /// <summary>Gets or sets the error message resource name to use in order to look up the <see cref="P:System.ComponentModel.DataAnnotations.ValidationAttribute.ErrorMessageResourceType" /> property value if validation fails.</summary>
        /// <returns>The error message resource that is associated with a validation control.</returns>
        public string ErrorMessageResourceName
        {
            get => this._errorMessageResourceName;
            set
            {
                if (this._errorMessage != null)
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_AlreadyInExplicitMode);
                if (this._errorMessageResourceName != null)
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_PropertyCannotBeSetMoreThanOnce);
                this._errorMessageResourceName = !string.IsNullOrEmpty(value) ? value : throw new ArgumentException(DataAnnotationsResources.ValidationAttribute_ValueCannotBeNullOrEmpty, nameof(value));
                this.SetResourceAccessorByPropertyLookup();
            }
        }

        private void SetResourceAccessorByPropertyLookup()
        {
            if (this._errorMessageResourceType != null && this._errorMessageResourceName != null)
            {
                PropertyInfo property = this._errorMessageResourceType.GetProperty(this._errorMessageResourceName, BindingFlags.Static | BindingFlags.Public);
                if (property == null)
                    throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, DataAnnotationsResources.ValidationAttribute_ResourceTypeDoesNotHaveProperty, (object)this._errorMessageResourceType.FullName, (object)this._errorMessageResourceName));
                this.ResourceAccessor = property.PropertyType == typeof(string) ? (Func<string>)(() => (string)property.GetValue((object)null, (object[])null)) : throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, DataAnnotationsResources.ValidationAttribute_ResourcePropertyNotStringType, (object)property.Name, (object)this._errorMessageResourceType.FullName));
                this._resourceModeAccessorIncomplete = false;
            }
            else
                this._resourceModeAccessorIncomplete = true;
        }

        /// <summary>Gets or sets an error message to associate with a validation control if validation fails.</summary>
        /// <returns>The error message that is associated with the validation control.</returns>
        public string ErrorMessage
        {
            get => this._errorMessage;
            set
            {
                if (this._errorMessageResourceName != null || this._errorMessageResourceType != null)
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_AlreadyInResourceMode);
                if (this._errorMessage != null)
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_PropertyCannotBeSetMoreThanOnce);
                this._errorMessage = !string.IsNullOrEmpty(value) ? value : throw new ArgumentException(DataAnnotationsResources.ValidationAttribute_ValueCannotBeNullOrEmpty, nameof(value));
                this.ResourceAccessor = (Func<string>)(() => this._errorMessage);
            }
        }

        /// <summary>Applies formatting to an error message, based on the data field where the error occurred.</summary>
        /// <param name="name">The name to include in the formatted message.</param>
        /// <returns>An instance of the formatted error message.</returns>
        public virtual string FormatErrorMessage(string name) => string.Format((IFormatProvider)CultureInfo.CurrentCulture, this.ErrorMessageString, (object)name);

        /// <summary>Determines whether the specified value of the object is valid.</summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// <see langword="true" /> if the specified value is valid; otherwise, <see langword="false" />.</returns>
        public abstract bool IsValid(object value);

        /// <summary>Validates the specified object.</summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <param name="name">The name to include in the error message.</param>
        /// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">
        /// <paramref name="value" /> is not valid.</exception>
        public void Validate(object value, string name)
        {
            if (!this.IsValid(value))
                throw new ValidationException(this.FormatErrorMessage(name), this, value);
        }
    }
}
