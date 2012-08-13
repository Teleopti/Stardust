using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Class to help when filtering
    /// </summary>
    [Serializable]
    public class FilterAdvancedTupleItem
    {
        private string _text;
        private object _value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public FilterAdvancedTupleItem(string text, object value)
        {
            _text = text;
            _value = value;
        }

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Value
        /// </summary>
        public object Value
        {
            get { return _value; }
        }
    }
}
