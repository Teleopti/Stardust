using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Display type for chart series
    /// </summary>
    [Serializable]
    public enum ChartSeriesDisplayType
    {
        /// <summary>
        /// Line
        /// </summary>
        Line = 0,
        /// <summary>
        /// Bar
        /// </summary>
        Bar = 5
    }
}