using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftDistributionGrid
	{
        IDistributionInformationExtractor ExtractorModel { get; }
	}

	public class ShiftDistributionGridPresenter
	{
		private IShiftDistributionGrid _view;
        private int _sortColumn;
        private bool _sortAscending;
	    private readonly IList<ShiftDistribution> _shiftDistributionList;
        private IList<DateOnly> _sortedDates; 

	    public ShiftDistributionGridPresenter(IShiftDistributionGrid view, IList<ShiftDistribution> shiftDistributionList)
		{
		    _view = view;
		    _shiftDistributionList = shiftDistributionList;
		}

	    public int? ShiftCategoryCount(DateOnly date, IShiftCategory shiftCategory)
	    {
	        foreach (var shiftDistribution in _shiftDistributionList)
	        {
	            if (shiftDistribution.DateOnly.Equals(date))
	            {
	                if (shiftDistribution.ShiftCategory.Equals(shiftCategory))
	                {
	                    return  shiftDistribution.Count;
	                }
	            }
	        }
	        return null;
	    }

        public void ReSort()
        {
            _sortAscending = !_sortAscending;
            Sort(_sortColumn);
        }

        public void Sort(int colIndex)
        {
            if (colIndex == _sortColumn)
                _sortAscending = !_sortAscending;
            else
                _sortAscending = true;

            _sortColumn = colIndex;

            var model = _view.ExtractorModel;
            var shiftDistributions = model.GetShiftDistribution();

            if (colIndex == 0)
            {

                if (_sortAscending)
                {
                    _sortedDates = (from DateOnly dateOnly  in model.Dates
                                    orderby dateOnly ascending
                                    select dateOnly).ToList();
                }
                else
                {
                    _sortedDates = (from DateOnly dateOnly in model.Dates
                                    orderby dateOnly descending
                                    select dateOnly).ToList();
                }
            }

            else
            {
                var shiftCategory = model.ShiftCategories[_sortColumn - 1];

                if (_sortAscending)
                {
                    _sortedDates = (from ShiftDistribution s in shiftDistributions
                                             orderby s.Count ascending
                                             where s.ShiftCategory.Equals(shiftCategory)
                                             select s.DateOnly ).ToList();
                }
                else
                {
                    _sortedDates = (from ShiftDistribution s in shiftDistributions
                                             orderby s.Count descending 
                                             where s.ShiftCategory.Equals(shiftCategory)
                                             select s.DateOnly).ToList();
                }

                foreach (var dateOnly in model.Dates )
                {
                    if (_sortedDates != null && !_sortedDates.Contains(dateOnly))
                    {
                        if (_sortAscending)
                            _sortedDates.Insert(0, dateOnly);
                        else
                            _sortedDates.Add(dateOnly );
                    }
                }
            }
        }

        public IList<DateOnly > SortedDates()
        {
            if (_sortedDates == null) _sortedDates = _view.ExtractorModel.Dates  ;
            return _sortedDates;
        }

	}
}
