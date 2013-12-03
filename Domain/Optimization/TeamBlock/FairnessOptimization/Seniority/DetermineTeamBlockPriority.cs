using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IDetermineTeamBlockPriority
    {
        ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos,
                                                           IList<IShiftCategory> shiftCategories);
    }

    public class DetermineTeamBlockPriority : IDetermineTeamBlockPriority
    {
        //private readonly ISelectedAgentPoints _selectedAgentPoints;
        private readonly IShiftCategoryPoints _shiftCategoryPoints;
	    private readonly ISeniorityExtractor _seniorityExtractor;

		//public DetermineTeamBlockPriority(ISelectedAgentPoints selectedAgentPoints, IShiftCategoryPoints shiftCategoryPoints)
		//{
		//	_selectedAgentPoints = selectedAgentPoints;
		//	_shiftCategoryPoints = shiftCategoryPoints;
		//}

		public DetermineTeamBlockPriority(ISeniorityExtractor seniorityExtractor, IShiftCategoryPoints shiftCategoryPoints)
		{
			_seniorityExtractor = seniorityExtractor;
			_shiftCategoryPoints = shiftCategoryPoints;
		}

        public ITeamBlockPriorityDefinitionInfo CalculatePriority(IList<ITeamBlockInfo> teamBlockInfos, IList<IShiftCategory> shiftCategories)
        {
	        var teamInfos = teamBlockInfos.Select(teamBlockInfo => teamBlockInfo.TeamInfo).ToList();
	        var seniorityInfos = _seniorityExtractor.ExtractSeniority(teamInfos);

            var teamBlockPriorityDefinitionInfo = new TeamBlockPriorityDefinitionInfo();
            foreach (ITeamBlockInfo teamBlockInfo in teamBlockInfos)
            {
                //var prioritiseAgentForTeamBlock = new PrioritiseAgentForTeamBlock(_selectedAgentPoints);
                var priortiseShiftCategoryForTeamBlock = new PrioritiseShiftCategoryForTeamBlock(_shiftCategoryPoints);
                //extractAgentAndShiftCategoryPriority(teamBlockInfo, prioritiseAgentForTeamBlock, priortiseShiftCategoryForTeamBlock);
				extractAgentAndShiftCategoryPriority(teamBlockInfo, priortiseShiftCategoryForTeamBlock);
				//if (!prioritiseAgentForTeamBlock.PrioritiseAgentList.Any() || !priortiseShiftCategoryForTeamBlock.PrioritiseShiftCategoryList.Any())
				//	continue;

				if (!seniorityInfos.Any() || !priortiseShiftCategoryForTeamBlock.PrioritiseShiftCategoryList.Any())
					continue;

                //var teamBlockInfoPriority = new TeamBlockInfoPriority(teamBlockInfo,  prioritiseAgentForTeamBlock.AveragePriority, priortiseShiftCategoryForTeamBlock.AveragePriority);

	            var seniorityInfo = seniorityInfos[teamBlockInfo.TeamInfo];
				var teamBlockInfoPriority = new TeamBlockInfoPriority(teamBlockInfo, seniorityInfo.Seniority, priortiseShiftCategoryForTeamBlock.AveragePriority);
                teamBlockPriorityDefinitionInfo.AddItem(teamBlockInfoPriority);
            }
            return teamBlockPriorityDefinitionInfo;
        }

        private void extractAgentAndShiftCategoryPriority(ITeamBlockInfo teamBlockInfo,
                                                          //IPrioritiseAgentForTeamBlock prioritiseAgentForTeamBlock,
                                                          IPrioritiseShiftCategoryForTeamBlock
                                                              prioritiseShiftCategoryForTeamBlock)
        {
            //prioritiseAgentForTeamBlock.GetPriortiseAgentByStartDate( teamBlockInfo.TeamInfo.GroupPerson.GroupMembers.ToList());
            var shiftCategories = new List<IShiftCategory>();
            foreach (IScheduleMatrixPro matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndPeriod(teamBlockInfo.BlockInfo.BlockPeriod))
            {
                foreach (DateOnly day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                {
                    IScheduleDayPro scheduleDay = matrix.GetScheduleDayByKey(day);
                    if (scheduleDay != null && scheduleDay.DaySchedulePart() != null &&
                        scheduleDay.DaySchedulePart().GetEditorShift() != null)
                    {
                        IShiftCategory sc = scheduleDay.DaySchedulePart().GetEditorShift().ShiftCategory;
                        if (!shiftCategories.Contains(sc))
                            shiftCategories.Add(sc);
                    }
                }
            }
            prioritiseShiftCategoryForTeamBlock.GetPrioritiseShiftCategories(shiftCategories);
        }

    }
}