using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookabookWPF.Services
{
    public class Filter
    {
        public PropertyInfo PropertyInfo { get; set; }
        public string WhereCondition { get; set; }

        public Filter(PropertyInfo propertyInfo, string whereCondition)
        {
            PropertyInfo = propertyInfo;
            WhereCondition = whereCondition;
        }
    }
}
