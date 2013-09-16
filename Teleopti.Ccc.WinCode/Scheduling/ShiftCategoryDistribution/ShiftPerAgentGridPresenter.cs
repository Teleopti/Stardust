using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftPerAgentGrid
	{
		IDistributionInformationExtractor ExtractorModel { get; }
		ISchedulerStateHolder SchedulerState { get; }
	}

	public class ShiftPerAgentGridPresenter
	{
		private readonly IShiftPerAgentGrid _view;
		private int _sortColumn;
		private bool _sortAscending;
		private IList<IPerson> _sortedPersonInvolved; 

		public ShiftPerAgentGridPresenter(IShiftPerAgentGrid view)
		{
			_view = view;
			_sortColumn = 0;
			_sortAscending = true;
			_sortedPersonInvolved = null;
		}

		public int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory, ICachedNumberOfEachCategoryPerPerson cachedNumberOfEachCategoryPerPerson)
		{
			int value;
			if (!cachedNumberOfEachCategoryPerPerson.GetValue(person).TryGetValue(shiftCategory, out value))
				value = 0;

			return value;
		}

		public void ReSort()
		{
			_sortedPersonInvolved = _view.ExtractorModel.PersonInvolved;
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
			var shiftCategoryList = model.GetShiftCategories();

			if (shiftCategoryList.Count <= (_sortColumn - 1) || shiftCategoryList.Count == 0)
			{
				return;	
			}
			
			if (colIndex == 0)
			{
				var schedulerState = _view.SchedulerState;
				_sortedPersonInvolved = model.PersonInvolved.OrderByWithDirection(schedulerState.CommonAgentName, !_sortAscending).ToList();
			}

			else
			{
				var shiftCategory = shiftCategoryList[_sortColumn - 1];

				_sortedPersonInvolved = model.GetAgentsSortedByNumberOfShiftCategories(shiftCategory);

				if (_sortAscending)
					_sortedPersonInvolved = new List<IPerson>(_sortedPersonInvolved.Reverse());
			}
		}

		public IList<IPerson> SortedPersonInvolved()
		{
			if (_sortedPersonInvolved == null) _sortedPersonInvolved = _view.ExtractorModel.PersonInvolved;
			return _sortedPersonInvolved;
		}
	}
}
