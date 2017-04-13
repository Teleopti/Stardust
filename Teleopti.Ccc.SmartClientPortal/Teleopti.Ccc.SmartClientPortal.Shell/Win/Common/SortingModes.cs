#region Imports

using System;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Represents a SortingModes (Ascending | Descending)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), Flags]
    [Serializable]
    public enum SortingModes
    {
        Ascending,
        Descending,
    }
}
