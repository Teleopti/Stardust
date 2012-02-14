using System;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Chart
{
    public class ChartSeriesSettingMicke65
    {
        [Serializable]
        public class ChartSeriesSetting
        {
            private readonly string _displayKey;
            private Color _color;
            private ChartSeriesDisplayType _seriesType;
            private bool _enabled;
            private AxisLocation _axisLocation;

            private ChartSeriesSetting(){}

            public ChartSeriesSetting(string displayKey, Color color, ChartSeriesDisplayType seriesType, bool visible, AxisLocation axisLocation)
                : this()
            {
                _displayKey = displayKey;
                _color = color;
                _seriesType = seriesType;
                _enabled = visible;
                _axisLocation = axisLocation;
            }


            public virtual string DisplayKey
            {
                get { return _displayKey; }
            }

            public virtual Color Color
            {
                get { return _color; }
                set { _color = value; }
            }

            public virtual ChartSeriesDisplayType SeriesType
            {
                get { return _seriesType; }
                set { _seriesType = value; }
            }

            public virtual bool Enabled
            {
                get { return _enabled; }
                set { _enabled = value; }
            }

            public virtual AxisLocation AxisLocation
            {
                get { return _axisLocation; }
                set { _axisLocation = value; }
            }

            public virtual object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}
