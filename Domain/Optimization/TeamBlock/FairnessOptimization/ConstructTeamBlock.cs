using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IConstructTeamBlock
    {
	    IList<ITeamBlockInfo> Construct(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
	                                    IList<IPerson> selectedPersons, bool useTeamBlockPerOption,
	                                    BlockFinderType blockFinderTypeForAdvanceScheduling,
	                                    IGroupPageLight groupOnGroupPageForTeamBlockPer);
    }

    /// <summary>
    /// INFO: This class can be moved to TeamBlock code as it serves more general purpose
    /// </summary>
    public class ConstructTeamBlock : IConstructTeamBlock
    {
        private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
        private readonly ITeamInfoFactory _teamInfoFactory;

        public ConstructTeamBlock(ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory)
        {
            _teamInfoFactory = teamInfoFactory;
            _teamBlockInfoFactory = teamBlockInfoFactory;
        }

        public IList<ITeamBlockInfo> Construct(IList<IScheduleMatrixPro> allPersonMatrixList,
                                               DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
											   bool useTeamBlockPerOption, BlockFinderType blockFinderTypeForAdvanceScheduling,
												IGroupPageLight groupOnGroupPageForTeamBlockPer)
        {
            var listOfAllTeamBlock = new List<ITeamBlockInfo>();

            foreach (DateOnly datePointer in selectedPeriod.DayCollection())
            {
                if (listOfAllTeamBlock.Count(s => s.BlockInfo.BlockPeriod.DayCollection().Contains(datePointer)) > 0)
                    continue;
                var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
                foreach (IPerson selectedPerson in selectedPersons)
                {
                    ITeamInfo teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod,
                                                                         allPersonMatrixList);
                    if (teamInfo != null)
                        allTeamInfoListOnStartDate.Add(teamInfo);
                }

                foreach (
                    ITeamInfo teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
                {
                    if (teamInfo == null) continue;
                    //if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson))
                    //    continue;

                    bool singleAgentTeam = groupOnGroupPageForTeamBlockPer != null &&
                                           groupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";

                    ITeamBlockInfo teamBlockInfo;
						  if (schedulingOptions.UseBlock)
                        teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
                                                                                  blockFinderTypeForAdvanceScheduling,
                                                                                  singleAgentTeam, allPersonMatrixList);
                    else
                        teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
                                                                                  BlockFinderType.SingleDay,
                                                                                  singleAgentTeam, allPersonMatrixList);
                    if (teamBlockInfo == null) continue;

                    listOfAllTeamBlock.Add(teamBlockInfo);
                }
            }
            return listOfAllTeamBlock;
        }
        
    }
}