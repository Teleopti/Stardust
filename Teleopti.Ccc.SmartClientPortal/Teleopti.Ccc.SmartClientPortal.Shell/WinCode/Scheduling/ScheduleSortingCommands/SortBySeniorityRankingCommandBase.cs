using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortBySeniorityRankingCommandBase
    {
	    private readonly SchedulingScreenState _schedulerState;

		public SortBySeniorityRankingCommandBase(SchedulingScreenState schedulerState)
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
			return _schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Values.ToList();
		}

		private void writeBackSortedListsToScheduler(IEnumerable<IPerson> personList)
        {
			_schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Clear();
			foreach (var person in personList)
			{
				if (person.Id != null)
					_schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Add(person.Id.Value, person);
			}
        }
    }
}