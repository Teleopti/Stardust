using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Common
{
    public enum FilterCriteriaType
    {
        /// <summary>
        /// text
        /// </summary>
        Text,
        /// <summary>
        /// hour:min
        /// </summary>
        HourMin,
        /// <summary>
        ///  time
        /// </summary>
        Time,
        /// <summary>
        /// date
        /// </summary>
        Date,
        /// <summary>
        /// list with values
        /// </summary>
        List,
        /// <summary>
        /// Number
        /// </summary>
        Number
    }

    public class FilterAdvancedSetting
    {
        private string _text;
        private FilterAdvancedTupleItem _filterOn;
        private IList<FilterAdvancedTupleItem> _filterOperands;
        private FilterCriteriaType _filterCriteriaType;
        private IList<FilterAdvancedTupleItem> _criteriaList;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filterOn"></param>
        /// <param name="filterOperands"></param>
        /// <param name="filterCriteriaType"></param>
        /// <param name="criteriaList"></param>
        public FilterAdvancedSetting(FilterAdvancedTupleItem filterOn, IList<FilterAdvancedTupleItem> filterOperands,
             FilterCriteriaType filterCriteriaType, IList<FilterAdvancedTupleItem> criteriaList)
        {
            this._text = filterOn.Text;
            this._filterOn = filterOn;
            this._filterOperands = filterOperands;
            this._filterCriteriaType = filterCriteriaType;
            this._criteriaList = criteriaList;
        }

        /// <summary>
        /// Text for the filter
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Filter on
        /// </summary>
        public FilterAdvancedTupleItem FilterOn
        {
            get { return _filterOn; }
        }

        /// <summary>
        /// Operand
        /// </summary>
        public IList<FilterAdvancedTupleItem> FilterOperands
        {
            get { return _filterOperands; }
        }

        /// <summary>
        /// Type of criteria
        /// </summary>
        public FilterCriteriaType FilterCriteriaType
        {
            get { return _filterCriteriaType; }
        }

        /// <summary>
        /// List to fill criteria combo with
        /// </summary>
        public IList<FilterAdvancedTupleItem> FilterCriteriaList
        {
            get { return _criteriaList; }
        }
    }
}
