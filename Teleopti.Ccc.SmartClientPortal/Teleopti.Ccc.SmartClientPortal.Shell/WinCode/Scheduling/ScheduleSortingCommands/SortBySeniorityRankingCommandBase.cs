using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortBySeniorityRankingCommandBase
    {
	    private readonly ISchedulerStateHolder _schedulerState;

		public SortBySeniorityRankingCommandBase(ISchedulerStateHolder schedulerState)
	    {
		    _schedulerState = schedulerState;
	    }

	    protected void Execute(Func<IEnumerable<IPerson>, IEnumerable<IPerson>> sortingMethod)
	    {
			var personList = extractPersonListFromScheduler();
			var sortedPersonList = sortingMethod.Invoke(personList);
			writeBackSortedListsToScheduler(sortedPersonList);
	    }

		private IEnumerable<IPerson> extractPersonListFromScheduler()
		{
			return _schedulerState.FilteredAgentsDictionary.Values.ToList();
		}

		private void writeBackSortedListsToScheduler(IEnumerable<IPerson> personList)
        {
			_schedulerState.FilteredAgentsDictionary.Clear();
			foreach (var person in personList)
			{
				if (person.Id != null)
					_schedulerState.FilteredAgentsDictionary.Add(person.Id.Value, person);
			}
        }
    }
}