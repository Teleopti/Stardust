using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftFairnessGrid
	{
		IDistributionInformationExtractor ExtractorModel { get; }
	}

	public class ShiftFairnessGridPresenter
	{
		private readonly IShiftFairnessGrid _view;
		private int _sortColumn;
		private bool _sortAscending;
		private IList<IShiftCategory> _sortedShiftCategories; 

		public ShiftFairnessGridPresenter(IShiftFairnessGrid view)
		{
			_view = view;
			_sortedShiftCategories = null;
		}


		public double CalculateTotalStandardDeviation(IList<ShiftFairness> fairnessList)
		{
			var total = fairnessList.Sum(shiftFairness => shiftFairness.StandardDeviationValue);
			return total;
		}

		public void Sort(int colIndex)
		{
			if (colIndex == _sortColumn) 
				_sortAscending = !_sortAscending;
			else
				_sortAscending = true;

			_sortColumn = colIndex;

			var model = _view.ExtractorModel;
			var shiftFairness = model.GetShiftFairness();

			if (colIndex == 0)
			{
				if (_sortAscending)
				{
					_sortedShiftCategories = (from IShiftCategory s in model.ShiftCategories
											 orderby s.Description.Name  ascending
											 select s).ToList();
				}
				else
				{
					_sortedShiftCategories = (from IShiftCategory s in model.ShiftCategories
											  orderby s.Description.Name descending 
											  select s).ToList();
				}
			}

			else
			{
				if (_sortAscending)
				{
					if (_sortColumn == 1)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												 orderby s.MinimumValue ascending 
												 select s.ShiftCategory).ToList();	
					}

					if (_sortColumn == 2)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.MaximumValue ascending
												  select s.ShiftCategory).ToList();	
					}

					if (_sortColumn == 3)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.AverageValue ascending
												  select s.ShiftCategory).ToList();	
					}

					if (_sortColumn == 4)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.StandardDeviationValue ascending
												  select s.ShiftCategory).ToList();	
					}
				}
				else
				{
					if (_sortColumn == 1)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.MinimumValue descending 
												  select s.ShiftCategory).ToList();
					}

					if (_sortColumn == 2)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.MaximumValue descending
												  select s.ShiftCategory).ToList();
					}

					if (_sortColumn == 3)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.AverageValue descending
												  select s.ShiftCategory).ToList();
					}

					if (_sortColumn == 4)
					{
						_sortedShiftCategories = (from ShiftFairness s in shiftFairness
												  orderby s.StandardDeviationValue descending
												  select s.ShiftCategory).ToList();
					}	
				}

				foreach (var shiftCategory in model.ShiftCategories)
				{
					if (_sortedShiftCategories != null && !_sortedShiftCategories.Contains(shiftCategory))
					{
						if (_sortAscending)
							_sortedShiftCategories.Insert(0, shiftCategory);
						else
							_sortedShiftCategories.Add(shiftCategory);
					}
				}
			}
		}

		public IList<IShiftCategory> SortedShiftCategories()
		{
			if (_sortedShiftCategories == null) _sortedShiftCategories = _view.ExtractorModel.ShiftCategories;
			return _sortedShiftCategories;
		}
	}
}
