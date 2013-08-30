using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftFairnessGrid
	{
		IDistributionInformationExtractor ExtractorModel { get; }
	}

	public enum ShiftFairnessGridColumns
	{
		ShiftCategory = 0,
		MinimumValue = 1,
		MaximumValue = 2,
		AverageValue = 3,
		StandardDeviationValue = 4
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
			_sortColumn = 0;
		}

		public int ColCount
		{
			get { return Enum.GetValues(typeof(ShiftFairnessGridColumns)).Cast<Int32>().Last(); }
		}

		public double CalculateTotalStandardDeviation(IList<ShiftFairness> fairnessList)
		{
			var total = fairnessList.Sum(shiftFairness => shiftFairness.StandardDeviationValue);
			return total;
		}

		public void ReSort()
		{
			_sortedShiftCategories = _view.ExtractorModel.ShiftCategories;
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
			var shiftFairness = model.GetShiftFairness();

			if (colIndex == (int)ShiftFairnessGridColumns.ShiftCategory) _sortedShiftCategories = model.ShiftCategories.OrderByWithDirection(s => s.Description.Name, !_sortAscending).ToList();
			else
			{
				if (_sortColumn == (int) ShiftFairnessGridColumns.MinimumValue) 
					_sortedShiftCategories = shiftFairness.OrderByWithDirection(s => s.MinimumValue, !_sortAscending).Select(s => s.ShiftCategory).ToList();
				if (_sortColumn == (int)ShiftFairnessGridColumns.MaximumValue) 
					_sortedShiftCategories = shiftFairness.OrderByWithDirection(s => s.MaximumValue, !_sortAscending).Select(s => s.ShiftCategory).ToList();
				if (_sortColumn == (int)ShiftFairnessGridColumns.AverageValue) 
					_sortedShiftCategories = shiftFairness.OrderByWithDirection(s => s.AverageValue, !_sortAscending).Select(s => s.ShiftCategory).ToList();
				if (_sortColumn == (int)ShiftFairnessGridColumns.StandardDeviationValue) 
					_sortedShiftCategories = shiftFairness.OrderByWithDirection(s => s.StandardDeviationValue, !_sortAscending).Select(s => s.ShiftCategory).ToList();
				
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
