using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfoForWeekDay
    {
        double GetWeekDayPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
        void AddItem(ITeamBlockInfoPriorityOnWeekDays teamBlockInfoPriorityOnWeekDays, ITeamBlockInfo teamBlockInfo, double priority);
        void SetWeekDayPriority(ITeamBlockInfo teamBlockInfo, double weekDayPriority);
        IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo { get; }
        IList<ITeamBlockInfo> HighToLowShiftCategoryPriority();
        ITeamBlockInfo ExtractAppropiateTeamBlock(ITeamBlockInfo teamBlock);
    }

    public class TeamBlockPriorityDefinitionInfoForWeekDay : ITeamBlockPriorityDefinitionInfoForWeekDay
    {
        private readonly IList<ITeamBlockInfoPriorityOnWeekDays> _teamBlockInfoPriorityOnWeekDaysList;
        private readonly IDictionary<ITeamBlockInfo, double> _teamBlockWeekDayPriority;

        public TeamBlockPriorityDefinitionInfoForWeekDay()
        {
            _teamBlockInfoPriorityOnWeekDaysList = new List<ITeamBlockInfoPriorityOnWeekDays>();
            _teamBlockWeekDayPriority = new Dictionary<ITeamBlockInfo, double>();
        }

        public void AddItem(ITeamBlockInfoPriorityOnWeekDays teamBlockInfoPriorityOnWeekDays, ITeamBlockInfo teamBlockInfo, double priority)
        {
            _teamBlockInfoPriorityOnWeekDaysList.Add(teamBlockInfoPriorityOnWeekDays);
            _teamBlockWeekDayPriority.Add(teamBlockInfo, priority);
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

        public ITeamBlockInfo ExtractAppropiateTeamBlock(ITeamBlockInfo teamBlock)
        {
            var providedWeekDayPoint = GetWeekDayPriorityOfBlock(teamBlock);
            var providedSenerioty =
                _teamBlockInfoPriorityOnWeekDaysList.FirstOrDefault(s => teamBlock == s.TeamBlockInfo).Seniority ;
            var canidateTeamBlock = _teamBlockInfoPriorityOnWeekDaysList.OrderBy(s => s.WeekDayPoints).FirstOrDefault(s => s.Seniority < providedSenerioty &&
                                                                                                                           s.WeekDayPoints > providedWeekDayPoint) ;
            if (canidateTeamBlock != null) return canidateTeamBlock.TeamBlockInfo;
            return null;
        }

        public double GetWeekDayPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        {
            return _teamBlockWeekDayPriority[teamBlockInfo];
        }

        public void SetWeekDayPriority(ITeamBlockInfo teamBlockInfo, double weekDayPriority)
        {
            _teamBlockWeekDayPriority[teamBlockInfo] = weekDayPriority;
        }
    }
}
