using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Common
{
    [Serializable]
    public class FilterBoxAdvancedFilter
    {
        private object _filterOn;
        private FilterAdvancedTupleItem _filterOperand;
        private FilterAdvancedTupleItem _filterCriteria;
        private object _filterValue;

        public FilterBoxAdvancedFilter(object filterOn, FilterAdvancedTupleItem filterOperand, FilterAdvancedTupleItem filterCriteria)
        {
            _filterOn = filterOn;
            _filterOperand = filterOperand;
            _filterCriteria = filterCriteria;
        }

        public FilterBoxAdvancedFilter(object filterOn, FilterAdvancedTupleItem filterOperand, FilterAdvancedTupleItem filterCriteria, object filterValue)
        {
            _filterOn = filterOn;
            _filterOperand = filterOperand;
            _filterCriteria = filterCriteria;
            _filterValue = filterValue;
        }

        public object FilterOn
        {
            get { return _filterOn; }
        }

        public FilterAdvancedTupleItem FilterOperand
        {
            get { return _filterOperand; }
        }

        public FilterAdvancedTupleItem FilterCriteria
        {
            get { return _filterCriteria; }
        }

        public object FilterValue
        {
            get { return _filterValue; }
        }
    }
}
