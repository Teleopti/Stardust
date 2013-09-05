#region Imports

using System;

#endregion

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents the name of a system setting
    /// </summary>
    [Flags]
    [Serializable]
    public enum SettingKeys
    {
        /// <summary>
        /// Default segment value for the raptor
        /// </summary>
        DefaultSegment = 1,
        
    }

}
