namespace Teleopti.Ccc.PayrollFormatter
{
    public class ItemFormat
    {
        private readonly string _element;
        private readonly string _xmlType;
        private string _format = string.Empty;
        private char _fill = ' ';
        private bool _quote;
        private Align _align = Align.Left;
        private int _length;

        public ItemFormat(string element, string xmlType)
        {
            _element = element;
            _xmlType = xmlType;
        }

        public string XmlType
        {
            get { return _xmlType; }
        }

        public string Element
        {
            get { return _element; }
        }

        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }

        public char Fill
        {
            get { return _fill; }
            set { _fill = value; }
        }

        public bool Quote
        {
            get { return _quote; }
            set { _quote = value; }
        }

        public Align Align
        {
            get { return _align; }
            set { _align = value; }
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }
    }
}