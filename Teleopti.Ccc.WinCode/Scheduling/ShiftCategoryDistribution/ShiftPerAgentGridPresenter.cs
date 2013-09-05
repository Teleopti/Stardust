using System.Collections.Generic;
using System.Linq;
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

		public int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory, IList<ShiftCategoryPerAgent> shiftCategoryPerAgentList)
		{
			return (from shiftCategoryPerAgent in shiftCategoryPerAgentList 
					where shiftCategoryPerAgent.Person.Equals(person) 
					where shiftCategoryPerAgent.ShiftCategory.Equals(shiftCategory) 
					select shiftCategoryPerAgent.Count).FirstOrDefault();
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
			var shiftCategoryPerAgent = model.GetShiftCategoryPerAgent();

			if (model.ShiftCategories.Count  <= (_sortColumn - 1) || model.ShiftCategories.Count == 0)
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
				var shiftCategory = model.ShiftCategories[_sortColumn - 1];

				_sortedPersonInvolved = shiftCategoryPerAgent.OrderByWithDirection(s => s.Count, !_sortAscending).Where(s=> s.ShiftCategory.Equals(shiftCategory)).Select(s => s.Person).ToList();

				foreach (var person in model.PersonInvolved)
				{
					if (_sortedPersonInvolved != null && !_sortedPersonInvolved.Contains(person))
					{
						if(_sortAscending)
							_sortedPersonInvolved.Insert(0, person);
						else
							_sortedPersonInvolved.Add(person);
					}
				}
			}
		}

		public IList<IPerson> SortedPersonInvolved()
		{
			if (_sortedPersonInvolved == null) _sortedPersonInvolved = _view.ExtractorModel.PersonInvolved;
			return _sortedPersonInvolved;
		}
	}
}
