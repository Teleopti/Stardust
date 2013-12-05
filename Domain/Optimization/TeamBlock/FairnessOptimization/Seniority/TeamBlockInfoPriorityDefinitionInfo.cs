using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        IEnumerable<double> HighToLowSeniorityList { get; }
        IEnumerable<int> HighToLowShiftCategoryPriorityList { get; }
        IEnumerable<int> LowToHighShiftCategoryPriorityList { get; }
        IEnumerable<double> LowToHighSeniorityList { get; }
        void Clear();
        ITeamBlockInfo BlockOnSeniority(double seniority);
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
    }

    public class TeamBlockPriorityDefinitionInfo : ITeamBlockPriorityDefinitionInfo
    {
        private readonly IList<TeamBlockInfoPriority> _teamBlockInfoPriorityList;

        public TeamBlockPriorityDefinitionInfo()
        {
            _teamBlockInfoPriorityList = new List<TeamBlockInfoPriority>( );
        }

        public void AddItem(TeamBlockInfoPriority teamBlockInfoPriority)
        {
            _teamBlockInfoPriorityList.Add(teamBlockInfoPriority);
        }

        public IEnumerable<double> HighToLowSeniorityList
        {
            get { return (_teamBlockInfoPriorityList.Select(s => s.Seniority)).ToList().OrderByDescending(s => s); }
        }

        public IEnumerable<int> HighToLowShiftCategoryPriorityList
        {
            get{return (_teamBlockInfoPriorityList.Select(s => s.ShiftCategoryPriority)).ToList().OrderByDescending(s => s);}
        }

        public IEnumerable<int> LowToHighShiftCategoryPriorityList
        {
            get { return (_teamBlockInfoPriorityList.Select(s => s.ShiftCategoryPriority)).ToList().OrderBy(s => s); }
        }

        public IEnumerable<double> LowToHighSeniorityList
        {
            get { return (_teamBlockInfoPriorityList.Select(s => s.Seniority)).ToList().OrderBy(s => s); }
        }

        public void Clear()
        {
            _teamBlockInfoPriorityList.Clear();
        }

        public ITeamBlockInfo BlockOnSeniority(double seniority)
        {
            return _teamBlockInfoPriorityList.FirstOrDefault(s => s.Seniority == seniority).TeamBlockInfo;
        }

        public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
        {
            return _teamBlockInfoPriorityList.FirstOrDefault(s => s.TeamBlockInfo == teamBlockInfo).ShiftCategoryPriority;
        }

    }
}
