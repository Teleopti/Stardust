using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Chart
{
    /// <summary>
    /// the eventargs that contains all the intel of the line in chart
    /// </summary>
    /// <remarks>
    /// Created by: ostenpe
    /// Created date: 2008-06-27
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Gridline")]
    public class GridlineInChartButtonEventArgs:EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GridlineInChartButtonEventArgs"/> is shown in chart.
        /// </summary>
        /// <value><c>shown</c> if enabled; otherwise, <c>hidden</c>.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the grid line chart style.
        /// </summary>
        /// <value>The grid line chart style. either bar or line</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public ChartSeriesDisplayType ChartSeriesStyle { get; set; }

        /// <summary>
        /// Gets or sets the  chart axis.
        /// </summary>
        /// <value>The chart axis.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public AxisLocation  GridToChartAxis { get; set; }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        /// <value>The color of the line.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public Color LineColor { get; set; }
    }
}