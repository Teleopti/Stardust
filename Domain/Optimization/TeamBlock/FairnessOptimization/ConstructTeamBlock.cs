using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IConstructTeamBlock
    {
	    IList<ITeamBlockInfo> Construct(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		    IEnumerable<IPerson> selectedPersons,
	                                    IBlockFinder blockFinder,
	                                    GroupPageLight groupOnGroupPageForTeamBlockPer);
    }

    /// <summary>
    /// INFO: This class can be moved to TeamBlock code as it serves more general purpose
    /// </summary>
    public class ConstructTeamBlock : IConstructTeamBlock
    {
        private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
	    private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
	    private readonly ITeamInfoFactory _teamInfoFactory;

        public ConstructTeamBlock(Func<ISchedulingResultStateHolder> schedulingResultStateHolder, ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory)
        {
	        _schedulingResultStateHolder = schedulingResultStateHolder;
	        _teamInfoFactory = teamInfoFactory;
            _teamBlockInfoFactory = teamBlockInfoFactory;
        }

        public IList<ITeamBlockInfo> Construct(IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
                                               DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons,
																															 IBlockFinder blockFinder,
												GroupPageLight groupOnGroupPageForTeamBlockPer)
        {
            var listOfAllTeamBlock = new List<ITeamBlockInfo>();

            foreach (DateOnly datePointer in selectedPeriod.DayCollection())
            {
                if (listOfAllTeamBlock.Count(s => s.BlockInfo.BlockPeriod.Contains(datePointer)) > 0)
                    continue;

                var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
                foreach (IPerson selectedPerson in selectedPersons)
                {
                    ITeamInfo teamInfo = _teamInfoFactory.CreateTeamInfo(_schedulingResultStateHolder().LoadedAgents, selectedPerson, selectedPeriod, allPersonMatrixList);
                    if (teamInfo != null)
                        allTeamInfoListOnStartDate.Add(teamInfo);
                }

                foreach (
                    ITeamInfo teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
                {
                    if (teamInfo == null) continue;

	                foreach (var groupMember in teamInfo.GroupMembers)
	                {
		                if(!selectedPersons.Contains(groupMember))
							teamInfo.LockMember(selectedPeriod , groupMember);
	                }

	                ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer, blockFinder);

                    if (teamBlockInfo == null) 
						continue;

	                var blockInfo = teamBlockInfo.BlockInfo;
					foreach (var dateOnly in blockInfo.BlockPeriod.DayCollection())
	                {
		                if(!selectedPeriod.Contains(dateOnly))
							blockInfo.LockDate(dateOnly);
	                }

                    listOfAllTeamBlock.Add(teamBlockInfo);
                }
            }

            return listOfAllTeamBlock;
        }
        
    }
}