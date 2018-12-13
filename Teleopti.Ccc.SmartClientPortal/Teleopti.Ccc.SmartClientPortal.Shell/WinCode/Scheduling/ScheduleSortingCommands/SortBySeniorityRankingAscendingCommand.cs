using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortBySeniorityRankingAscendingCommand : SortBySeniorityRankingCommandBase, 
		IScheduleSortCommand
    {
	    private readonly IRankedPersonBasedOnStartDate _rankingCalculator;

	    public SortBySeniorityRankingAscendingCommand(SchedulingScreenState schedulerState, IRankedPersonBasedOnStartDate rankingCalculator)
			:base(schedulerState)
	    {
		    _rankingCalculator = rankingCalculator;
	    }

	    public void Execute(DateOnly dateToExecuteOn)
	    {
			Execute(sortPersonList);
	    }

	    private IEnumerable<IPerson> sortPersonList(IEnumerable<IPerson> unsortedList)
	    {
			var rankedDic = _rankingCalculator.GetRankedPersonDictionary(unsortedList);

			var sortedPersonList =
				(from item in rankedDic
				orderby item.Value ascending 
				select item.Key).ToList();

			return sortedPersonList;
	    }
    }
}