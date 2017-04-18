using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.WinCode.Common.Chart
{
    /// <summary>
    /// Settings for one chart series
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-05-07
    /// </remarks>
    [Serializable]
    public class ChartSeriesSetting : IChartSeriesSetting
    {
        private readonly string _displayKey;
        private Color _color;
        private ChartSeriesDisplayType _seriesType;
        private bool _enabled;
        private AxisLocation _axisLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSeriesSetting"/> class.
        /// </summary>
        /// <param name="displayKey">The display key.</param>
        /// <param name="color">The color.</param>
        /// <param name="seriesType">Type of the series.</param>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        /// <param name="axisLocation">The axis location.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        public ChartSeriesSetting(string displayKey,Color color, ChartSeriesDisplayType seriesType, bool visible, AxisLocation axisLocation) : this()
        {
            _displayKey = displayKey;
            _color = color;
            _seriesType = seriesType;
            _enabled = visible;
            _axisLocation = axisLocation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSeriesSetting"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        protected ChartSeriesSetting()
        { }

        /// <summary>
        /// Gets the display key.
        /// </summary>
        /// <value>The display key.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-08
        /// </remarks>
        public virtual string DisplayKey
        {
            get { return _displayKey; }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Gets or sets the type of the series.
        /// </summary>
        /// <value>The type of the series.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual ChartSeriesDisplayType SeriesType
        {
            get { return _seriesType; }
            set { _seriesType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ChartSeriesSetting"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Gets or sets the axis location.
        /// </summary>
        /// <value>The axis location.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-07
        /// </remarks>
        public virtual AxisLocation AxisLocation
        {
            get { return _axisLocation; }
            set { _axisLocation = value; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-10-09
        /// </remarks>
        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        public virtual IChartSeriesSetting NoneEntityClone()
        {
            IChartSeriesSetting retobj = EntityClone();
            //retobj.SetId(null);
            return retobj;
        }

        public virtual IChartSeriesSetting EntityClone()
        {
            return (IChartSeriesSetting)MemberwiseClone();
        }
    }
}