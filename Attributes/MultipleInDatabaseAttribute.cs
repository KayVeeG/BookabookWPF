using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookabookWPF.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MultipleInDatabaseAttribute : Attribute
    {

        public MultipleInDatabaseAttribute()
        { }
    }
}
