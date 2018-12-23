using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FakeExtractor.Helpers
{
    /// <summary>
    /// Helps apply value to object property
    /// </summary>
    public static class ApplyValueExtension
    {
        /// <summary>
        /// Apply value to object
        /// </summary>
        /// <typeparam name="T">Generic type of object</typeparam>
        /// <param name="obj">the instance of object</param>
        /// <param name="key">the name of property</param>
        /// <param name="value">the value that need to set</param>
        /// <returns></returns>
        public static bool ApplyValue<T>(this T obj, string key, string value) where T : class
        {
            if (obj == null ||
                string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var property = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => key.EqualTo(x.Name));
            if (property == null)
            {
                return false;
            }

            var converted = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(value);
            property.SetValue(obj, converted);
            return true;
        }
    }
}