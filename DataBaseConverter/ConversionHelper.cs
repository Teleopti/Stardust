using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Helper class
    /// </summary>
    public sealed class ConversionHelper
    {
        private ConversionHelper(){}

        /// <summary>
        /// Maps the string.
        /// </summary>
        /// <param name="textToMap">The string to map.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-06-10
        /// </remarks>
        public static string MapString(string textToMap, int maxLength)
        {

            if (textToMap.Length > maxLength)
                return textToMap.Remove(maxLength);

            return textToMap;
        }
    }
}
