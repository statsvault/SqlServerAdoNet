using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Reflection helpers.
    /// </summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Gets an attribute's property value.  Returns the first one found.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute whose property value is to be returned.</typeparam>
        /// <typeparam name="TValue">Attribute property value.</typeparam>
        /// <param name="type">Type whose attribute is to be returned.</param>
        /// <param name="valueSelector">Function delegate specifying which attribute property value to return.</param>
        /// <returns>TValue</returns>
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var attr = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            return GetAttributeValue(attr, valueSelector);
        }

        /// <summary>
        /// Gets an attribute's property value.  Returns the first one found.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute whose property value is to be returned.</typeparam>
        /// <typeparam name="TValue">Attribute property value.</typeparam>
        /// <param name="prop">Property whose attribute is to be returned.</param>
        /// <param name="valueSelector">Function delegate specifying which attribute property value to return.</param>
        /// <returns>TValue</returns>
        public static TValue GetAttributeValue<TAttribute, TValue>(this PropertyInfo prop, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var attr = prop.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            return GetAttributeValue(attr, valueSelector);
        }

        /// <summary>
        /// Gets an attribute's property value.
        /// </summary>
        /// <typeparam name="TAttribute">Attribute whose property value is to be returned.</typeparam>
        /// <typeparam name="TValue">Attribute property value.</typeparam>
        /// <param name="attr">Attribute whose property value is to be returned.</param>
        /// <param name="valueSelector">Function delegate specifying which attribute property value to return.</param>
        /// <returns>TValue</returns>
        private static TValue GetAttributeValue<TAttribute, TValue>(TAttribute attr, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            if (attr != null)
            {
                return valueSelector(attr);
            }
            return default(TValue);
        }

        /// <summary>
        /// Get the property for the specified name.
        /// </summary>
        /// <typeparam name="T">Type whose property is to be returned.</typeparam>
        /// <param name="propName">Name of property.</param>
        /// <returns></returns>
        public static PropertyInfo GetProperty<T>(string propName)
        {
            return typeof(T).GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Gets the properties for the specified type.
        /// </summary>
        /// <typeparam name="T">Type whose properties are to be returned.</typeparam>
        /// <returns>PropertyInfo[]</returns>
        public static PropertyInfo[] GetProperties<T>()
        {
            return GetProperties(typeof(T));
        }

        /// <summary>
        /// Gets the properties for the specified type.
        /// </summary>
        /// <param name="type">Type whose properties are to be returned.</param>
        /// <returns>PropertyInfo[]</returns>
        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Gets the property name/value pairs of the specified object.
        /// </summary>
        /// <typeparam name="T">Type whose property name/value pairs are to be returned.</typeparam>
        /// <param name="entity">Specific instance whose property name/value pairs are to be returned.</param>
        /// <returns>Dictionary<string, object></returns>
        public static Dictionary<string, object> GetPropertyValues<T>(T entity)
        {
            var propValues = new Dictionary<string, object>();

            var props = GetProperties(entity.GetType());
            foreach (var prop in props)
            {
                propValues.Add(prop.Name, prop.GetValue(entity, null));
            }

            return propValues;
        }

        /// <summary>
        /// Gets the property names of the specified object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>List of property names.</returns>
        public static IEnumerable<string> GetPropertyNames<T>()
        {
            var props = GetProperties<T>();
            return props.Select(x => x.Name);
        }

        /// <summary>
        /// Get a list of property names and their corresponding database column names.
        /// </summary>
        /// <typeparam name="T">Type whose names are to be returned.</typeparam>
        /// <returns>List of property name and column name pairs.</returns>
        public static Dictionary<string, string> GetPropertyColumnNames<T>()
        {
            var colNames = new Dictionary<string, string>();

            var props = GetProperties<T>();
            foreach (var prop in props)
            {
                var colName = prop.GetAttributeValue((ColumnAttribute a) => a.Name) ?? prop.Name;
                colNames.Add(prop.Name, colName);
            }

            return colNames;
        }
    }
}

