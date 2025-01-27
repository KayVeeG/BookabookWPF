using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookabookWPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequiresValidationAttribute : Attribute
    {

        // Delegate for the validation method
        public delegate bool ValidationDelegate(object value);

        public ValidationDelegate ValidationMethod { get; set; }


        // Constructor
        public RequiresValidationAttribute(ValidationDelegate validationMethod)
        {
            ValidationMethod = validationMethod;
        }
    }
}
