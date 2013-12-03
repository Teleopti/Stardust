using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        IEnumerable<int> HighToLowAgentPriorityList { get; }
        IEnumerable<int> HighToLowShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighAgentPriorityList { get; }
        void Clear();
        ITeamBlockInfo BlockOnAgentPriority(int priority);
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
    }

    public class TeamBlockPriorityDefinitionInfo : ITeamBlockPriorityDefinitionInfo
    {
        private IList<TeamBlockInfoPriority> _teamBlockInfoPriorityList;

        public TeamBlockPriorityDefinitionInfo()
        {
            _teamBlockInfoPriorityList = new List<TeamBlockInfoPriority>( );
        }

        public void AddItem(TeamBlockInfoPriority teamBlockInfoPriority)
        {
            _teamBlockInfoPriorityList.Add(teamBlockInfoPriority);
        }

        public IEnumerable<int> HighToLowAgentPriorityList
        {
            get { return (_teamBlockInfoPriorityList.Select(s => s.Seniority)).ToList().OrderByDescending(s => s); }
        }

        public IEnumerable<int> HighToLowShiftCategoryPriorityList
        {
            get
            {
                return (_teamBlockInfoPriorityList.Select(s => s.ShiftCategoryPriority)).ToList().OrderByDescending(s => s);
            }
        }

        public IEnumerable<int> LowToHighShiftCategoryPriorityList
        {
            get { return (_teamBlockInfoPriorityList.Select(s => s.ShiftCategoryPriority)).ToList().OrderBy(s => s); }
        }

        public IEnumerable<int> LowToHighAgentPriorityList
        {
            get { return (_teamBlockInfoPriorityList.Select(s => s.Seniority)).ToList().OrderBy(s => s); }
        }

        public void Clear()
        {
            _teamBlockInfoPriorityList.Clear();
        }

        public ITeamBlockInfo BlockOnAgentPriority(int priority)
        {
            return _teamBlockInfoPriorityList.FirstOrDefault(s => s.Seniority == priority).TeamBlockInfo;
        }

        public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        {
            return _teamBlockInfoPriorityList.FirstOrDefault(s => s.TeamBlockInfo == teamBlockInfo).ShiftCategoryPriority;
        }

    }
}
