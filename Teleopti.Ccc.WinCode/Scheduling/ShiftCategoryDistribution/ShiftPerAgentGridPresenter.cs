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
			
			if (colIndex == 0)
			{
				var schedulerState = _view.SchedulerState;

				if (_sortAscending)
				{
					_sortedPersonInvolved = (from IPerson p in model.PersonInvolved
											 orderby schedulerState.CommonAgentName(p) ascending 
											 select p).ToList();
				}
				else
				{
					_sortedPersonInvolved = (from IPerson p in model.PersonInvolved
											 orderby schedulerState.CommonAgentName(p) descending 
											 select p).ToList();
				}
			}

			else
			{
				var shiftCategory = model.ShiftCategories[_sortColumn - 1];

				if (_sortAscending)
				{
					_sortedPersonInvolved = (from ShiftCategoryPerAgent s in shiftCategoryPerAgent
					                         orderby s.Count ascending
					                         where s.ShiftCategory.Equals(shiftCategory)
					                         select s.Person).ToList();
				}
				else
				{
					_sortedPersonInvolved = (from ShiftCategoryPerAgent s in shiftCategoryPerAgent
					                         orderby s.Count descending
					                         where s.ShiftCategory.Equals(shiftCategory)
					                         select s.Person).ToList();
				}

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
