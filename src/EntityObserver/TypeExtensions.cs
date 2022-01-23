using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EntityObserver
{
    /// <summary>
    /// 
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Determines if the current type is a tye that implements <see cref="IObserver"/>
        /// </summary>
        /// <param name="type">The type to examine.</param>
        /// <returns>true if the type implements <see cref="IObserver"/>; otherwise, false.</returns>
        public static bool IsObserver(this Type type)
        {
            return typeof(IObserver).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if the current type is a generic collection type,
        /// or a type that implements <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="type">The type to examine.</param>
        /// <returns>true if the type is generic and is assignable from <see cref="ICollection{T}"/>; otherwise, false.</returns>
        public static bool IsObserverCollection(this Type type) =>
            type.IsGenericType && typeof(ObserverCollection<>).IsAssignableFrom(type.GetGenericTypeDefinition());

        /// <summary>
        /// Creates a new empty <see cref="IList"/> of the current type.
        /// </summary>
        /// <param name="type">The type that the generic list represents.</param>
        /// <returns>A new empty <see cref="IList"/> instance of the of of the current type.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IList CreateList(this Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);

            var instance = Activator.CreateInstance(listType);

            if (instance is not IList list)
                throw new InvalidOperationException();

            return list;
        }

        public static IEnumerable<TAttribute> FindAttributes<TAttribute>(this Type type)
            where TAttribute : Attribute =>
            type.GetProperties().SelectMany(p => p.GetCustomAttributes(false).OfType<TAttribute>());

        public static TAttribute? FindAttribute<TAttribute>(this Type type, string propertyName)
            where TAttribute : Attribute =>
            type.GetProperty(propertyName)?.GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault();
    }
}