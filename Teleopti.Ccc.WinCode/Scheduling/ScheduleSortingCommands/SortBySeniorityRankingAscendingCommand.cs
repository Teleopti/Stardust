using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortBySeniorityRankingAscendingCommand : IScheduleSortCommand
    {
	    private readonly ISchedulerStateHolder _schedulerState;
	    private readonly IRankedPersonBasedOnStartDate _rankingCalculator;

	    public SortBySeniorityRankingAscendingCommand(ISchedulerStateHolder schedulerState, IRankedPersonBasedOnStartDate rankingCalculator)
	    {
		    _schedulerState = schedulerState;
		    _rankingCalculator = rankingCalculator;
	    }

	    public void Execute(DateOnly dateToExecuteOn)
	    {
			var unsortList = extractRankedPersonsFromScheduler();
		    var sortList = sortPersonRankDictionary(unsortList);
			writeBackSortedListsToScheduler(sortList);
	    }

		private IEnumerable<KeyValuePair<IPerson, int>> extractRankedPersonsFromScheduler()
		{
			var personList = _schedulerState.FilteredAgentsDictionary.Values.ToList();
			var rankedDic = _rankingCalculator.GetRankedPersonDictionary(personList);
			return rankedDic;
		}

	    private static IEnumerable<IPerson> sortPersonRankDictionary(IEnumerable<KeyValuePair<IPerson, int>> unsortDictionary)
	    {
			var sortedDictionary =
				(from item in unsortDictionary
				orderby item.Value ascending 
				select item.Key).ToList();

			return sortedDictionary;
	    }

		private void writeBackSortedListsToScheduler(IEnumerable<IPerson> sortedPersonList)
        {
			_schedulerState.FilteredAgentsDictionary.Clear();
			foreach (var person in sortedPersonList)
			{
				if (person.Id != null)
					_schedulerState.FilteredAgentsDictionary.Add(person.Id.Value, person);
			}
        }
    }
}