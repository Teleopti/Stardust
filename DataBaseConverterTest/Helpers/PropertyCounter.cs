using System;
using System.Reflection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.Helpers
{
    /// <summary>
    /// Counts the properties of an instance.
    /// </summary>
    public static class PropertyCounter
    {
        /// <summary>
        /// Counts the properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static int CountProperties(Type type)
        {
            InParameter.NotNull("type", type);
            PropertyInfo[] myPropertyInfo = type.GetProperties();

            return myPropertyInfo.Length;
        }
    }
}