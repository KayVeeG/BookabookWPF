using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookabookWPF
{
    public static class Utility
    {
        public static List<Type> FindClassesWithAttribute<T>() where T : Attribute
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0)
                .ToList();
        }
    }
}
