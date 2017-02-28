using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Settings for one series in a chart setting
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-06-23
    /// </remarks>
    public interface IChartSeriesSetting : ICloneableEntity<IChartSeriesSetting>
    {
        /// <summary>
        /// Gets the display key.
        /// </summary>
        /// <value>The display key.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-08
        /// </remarks>
        string DisplayKey { get; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        Color Color { get; set; }

        /// <summary>
        /// Gets or sets the type of the series.
        /// </summary>
        /// <value>The type of the series.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        ChartSeriesDisplayType SeriesType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IChartSeriesSetting"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the axis location.
        /// </summary>
        /// <value>The axis location.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        AxisLocation AxisLocation { get; set; }
    }
}