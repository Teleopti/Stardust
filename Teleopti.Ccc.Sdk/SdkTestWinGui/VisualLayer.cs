using System.Drawing;

namespace GridTest
{
    public class VisualLayer
    {
        private TimePeriod _period;
        private Color _color;
        private string _description;

        public VisualLayer(TimePeriod period, Color color, string description)
        {
            _period = period;
            _color = color;
            _description = description;
        }
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public TimePeriod Period
        {
            get { return _period; }
            set { _period = value; }
        }
    }
}