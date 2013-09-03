using System.Collections.Generic;

namespace Teleopti.Ccc.Win.Common.Controls.Filters
{
    internal struct FilterItem
    {
        private readonly object _valueItem;
        private readonly string _text;
        private readonly IList<TupleItem> _alternatives;

        public FilterItem(object valueItem, string text, IList<TupleItem> alternatives)
        {
            _valueItem = valueItem;
            _text = text;
            _alternatives = alternatives;
        }

        public object ValueItem
        {
            get { return _valueItem; }
        }

        public string Text
        {
            get { return _text; }
        }

        public IList<TupleItem> Alternatives
        {
            get { return _alternatives; }
        }
    }
}
