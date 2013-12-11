using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
	    void AddItem(ITeamBlockInfoPriority teamBlockInfoPriority);
	    void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, int shiftCategoryPriority);
	    IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo { get; }
		IList<ITeamBlockInfo> LowToHighSeniorityListBlockInfo { get; }
    }

	public class TeamBlockPriorityDefinitionInfo : ITeamBlockPriorityDefinitionInfo
	{
		private readonly IList<ITeamBlockInfoPriority> _teamBlockInfoPriorityList;

		public TeamBlockPriorityDefinitionInfo()
		{
			_teamBlockInfoPriorityList = new List<ITeamBlockInfoPriority>();
		}

		public void AddItem(ITeamBlockInfoPriority teamBlockInfoPriority)
		{
			_teamBlockInfoPriorityList.Add(teamBlockInfoPriority);
		}

		public IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo
		{
			get { return (_teamBlockInfoPriorityList.OrderByDescending(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
		}

		public IList<ITeamBlockInfo> LowToHighSeniorityListBlockInfo
		{
			get { return (_teamBlockInfoPriorityList.OrderBy(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
		}

		public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
		{
			var teamBlockInfoPriority = _teamBlockInfoPriorityList.FirstOrDefault(blockInfoPriority => blockInfoPriority.TeamBlockInfo.Equals(teamBlockInfo));
			return teamBlockInfoPriority != null ? teamBlockInfoPriority.ShiftCategoryPriority : 0;
		}

		public void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, int shiftCategoryPriority)
		{
			var teamBlockInfoPriority = _teamBlockInfoPriorityList.FirstOrDefault(blockInfoPriority => blockInfoPriority.TeamBlockInfo.Equals(teamBlockInfo));
			if (teamBlockInfoPriority == null) return;
			teamBlockInfoPriority.ShiftCategoryPriority = shiftCategoryPriority;
		}
	}
}
