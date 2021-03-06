﻿using RepoDb.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RepoDb.Extensions
{
    /// <summary>
    /// Contains the extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// Converts all properties of the type into an array of <see cref="Field"/> objects.
        /// </summary>
        /// <param name="type">The current type.</param>
        /// <returns>A list of <see cref="Field"/> objects.</returns>
        internal static IEnumerable<Field> AsFields(this Type type)
        {
            return PropertyCache.Get(type).AsFields();
        }

        /// <summary>
        /// Converts all properties of the type into an array of <see cref="ClassProperty"/> objects.
        /// </summary>
        /// <param name="type">The current type.</param>
        /// <returns>A list of <see cref="ClassProperty"/> objects.</returns>
        public static IEnumerable<ClassProperty> GetClassProperties(this Type type)
        {
            foreach (var property in type.GetProperties())
            {
                yield return new ClassProperty(property);
            }
        }

        /// <summary>
        /// Returns the underlying type of the current type. If there is no underlying type, this will return the current type.
        /// </summary>
        /// <param name="type">The current type to check.</param>
        /// <returns>The underlying type or the current type.</returns>
        public static Type GetUnderlyingType(this Type type)
        {
            return type != null ? Nullable.GetUnderlyingType(type) ?? type : null;
        }

        /// <summary>
        /// Returns the mapped property if the property is not present.
        /// </summary>
        /// <param name="type">The current type.</param>
        /// <param name="mappedName">The name of the property mapping.</param>
        /// <returns>The instance of <see cref="ClassProperty"/>.</returns>
        internal static ClassProperty GetPropertyByMapping(this Type type,
            string mappedName)
        {
            return PropertyCache.Get(type)
                .FirstOrDefault(p => string.Equals(p.GetMappedName(), mappedName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
