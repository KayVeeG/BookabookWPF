using System.ComponentModel;
using System.Reflection;
using SQLite;

namespace BookabookWPF.Services
{
    public abstract class ModelBase : INotifyPropertyChanged, ICloneable
    {

        private static readonly Type[] ignoringTypes =
        {
            typeof(PrimaryKeyAttribute)
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<PropertyInfo> GetDataProperties()
        {
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (property.GetCustomAttributes().Any(a => ignoringTypes.Contains(a.GetType()))) continue;
                yield return property;
            }

        }
    }
}
