using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockWeekDaySeniorityCalculator
    {
        double GetWeekDayPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
        void AddItem(TeamBlockSeniorityOnWeekDays teamBlockInfoPriorityOnWeekDays, ITeamBlockInfo teamBlockInfo, double priority);
        void SetWeekDayPriority(ITeamBlockInfo teamBlockInfo, double weekDayPriority);
        IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo { get; }
        IEnumerable<ITeamBlockInfo> ExtractAppropiateTeamBlock(ITeamBlockInfo teamBlock);
    }

    public class TeamBlockWeekDaySeniorityCalculator : ITeamBlockWeekDaySeniorityCalculator
    {
        private readonly IList<TeamBlockSeniorityOnWeekDays> _teamBlockInfoPriorityOnWeekDaysList;
        private readonly IDictionary<ITeamBlockInfo, double> _teamBlockWeekDayPriority;

        public TeamBlockWeekDaySeniorityCalculator()
        {
            _teamBlockInfoPriorityOnWeekDaysList = new List<TeamBlockSeniorityOnWeekDays>();
            _teamBlockWeekDayPriority = new Dictionary<ITeamBlockInfo, double>();
        }

        public void AddItem(TeamBlockSeniorityOnWeekDays teamBlockInfoPriorityOnWeekDays, ITeamBlockInfo teamBlockInfo, double priority)
        {
            _teamBlockInfoPriorityOnWeekDaysList.Add(teamBlockInfoPriorityOnWeekDays);
            _teamBlockWeekDayPriority.Add(teamBlockInfo, priority);
        }

        public IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo
        {
            get { return (_teamBlockInfoPriorityOnWeekDaysList.OrderByDescending(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
        }

        
        public IEnumerable<ITeamBlockInfo> ExtractAppropiateTeamBlock(ITeamBlockInfo teamBlock)
        {
            var providedWeekDayPoint = GetWeekDayPriorityOfBlock(teamBlock);
            var providedSenerioty =
                _teamBlockInfoPriorityOnWeekDaysList.FirstOrDefault(s => teamBlock == s.TeamBlockInfo).Seniority ;
            var canidateTeamBlocks = from t in _teamBlockInfoPriorityOnWeekDaysList
                                     where t.WeekDayPoints > providedWeekDayPoint && t.Seniority < providedSenerioty
                                     orderby t.WeekDayPoints descending
                                     select t.TeamBlockInfo;

            return canidateTeamBlocks;
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
