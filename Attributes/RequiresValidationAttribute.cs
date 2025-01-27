using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookabookWPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequiresValidationAttribute : Attribute
    {

        // Delegate for the validation method
        public delegate bool ValidationDelegate(object? value);

        public ValidationDelegate ValidationMethod { get; set; }
        public string? UserInstruction { get; set; }


        // Constructor
        public RequiresValidationAttribute(string validationMethodName, string? userInstruction = null)
        {
            // Split the full method name into parts
            string[] parts = validationMethodName.Split('.');
            // Extract the method name
            string methodName = parts[parts.Length - 1];
            // Extract the type name
            string typeName = string.Join(".", parts.Take(parts.Length - 1));

            // Get the type
            Type type = Type.GetType(typeName)!;
            // Get method info
            MethodInfo methodInfo = type.GetMethod(methodName)!;
            // Set the delegate
            ValidationMethod = (ValidationDelegate)Delegate.CreateDelegate(typeof(ValidationDelegate), methodInfo);

            // Set the user instruction
            UserInstruction = userInstruction;
        }
    }
}
