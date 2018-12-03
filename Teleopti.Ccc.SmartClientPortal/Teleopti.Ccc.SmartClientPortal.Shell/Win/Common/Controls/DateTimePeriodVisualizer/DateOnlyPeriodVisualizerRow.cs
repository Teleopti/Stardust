using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer
{
    public class DateOnlyPeriodVisualizerRow
    {
        private readonly string _text;
        private readonly IList<DateOnlyPeriod> _periods;
        private readonly Color _displayColor;

        public DateOnlyPeriodVisualizerRow(string text, IList<DateOnlyPeriod> periods, Color displayColor)
        {
            _text = text;
            _displayColor = displayColor;
            _periods = periods;
        }

        public Color DisplayColor
        {
            get { return _displayColor; }
        }

        public IList<DateOnlyPeriod> Periods
        {
            get { return _periods; }
        }

        public string Text
        {
            get { return _text; }
        }
    }
}