#region Imports

using System;

#endregion

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a value type of a system setting instance
    /// </summary>
    [Flags]
    [Serializable]
    public enum SettingValueTypes : int
    {
        /// <summary>
        /// Numeric value
        /// </summary>
        Integer = 1,
        /// <summary>
        /// Text values
        /// </summary>
        String = 2,
        /// <summary>
        /// Decimal values
        /// </summary>
        Double = 3,
    }

}
