using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfoPriorityForWeekDay
    {
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
        void AddItem(ITeamBlockInfoPriorityOnWeekDays teamBlockInfoPriorityOnWeekDays, ITeamBlockInfo teamBlockInfo, int priority);
        void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, int shiftCategoryPriority);
        IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo { get; }
        IList<ITeamBlockInfo> HighToLowShiftCategoryPriority();
    }

    public class TeamBlockPriorityDefinitionInfoPriorityForWeekDay : ITeamBlockPriorityDefinitionInfoPriorityForWeekDay
    {
        private readonly IList<ITeamBlockInfoPriorityOnWeekDays> _teamBlockInfoPriorityOnWeekDaysList;
        private readonly IDictionary<ITeamBlockInfo, int> _teamBlockShiftCategoryPriority;

        public TeamBlockPriorityDefinitionInfoPriorityForWeekDay()
        {
            _teamBlockInfoPriorityOnWeekDaysList = new List<ITeamBlockInfoPriorityOnWeekDays>();
            _teamBlockShiftCategoryPriority = new Dictionary<ITeamBlockInfo, int>();
        }

        public void AddItem(ITeamBlockInfoPriorityOnWeekDays teamBlockInfoPriorityOnWeekDays, ITeamBlockInfo teamBlockInfo, int priority)
        {
            _teamBlockInfoPriorityOnWeekDaysList.Add(teamBlockInfoPriorityOnWeekDays);
            _teamBlockShiftCategoryPriority.Add(teamBlockInfo, priority);
        }

        public IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo
        {
            get { return (_teamBlockInfoPriorityOnWeekDaysList.OrderByDescending(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
        }

        public IList<ITeamBlockInfo> HighToLowShiftCategoryPriority()
        {
            var result = _teamBlockInfoPriorityOnWeekDaysList.OrderByDescending(s => s.WeekDayPoints ).Select(s => s.TeamBlockInfo).ToList();
            return result;
        }

        public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        {
            return _teamBlockShiftCategoryPriority[teamBlockInfo];
        }

        public void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, int shiftCategoryPriority)
        {
            _teamBlockShiftCategoryPriority[teamBlockInfo] = shiftCategoryPriority;
        }
    }
}
