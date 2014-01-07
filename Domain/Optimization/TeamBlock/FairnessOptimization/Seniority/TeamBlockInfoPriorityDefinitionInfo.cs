using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
	    void AddItem(ITeamBlockInfoPriority teamBlockInfoPriority, ITeamBlockInfo teamBlockInfo, int priority);
	    void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, int shiftCategoryPriority);
	    IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo { get; }
	    IList<ITeamBlockInfo> HighToLowShiftCategoryPriority();
    }

	public class TeamBlockPriorityDefinitionInfo : ITeamBlockPriorityDefinitionInfo
	{
		private readonly IList<ITeamBlockInfoPriority> _teamBlockInfoPriorityList;
		private readonly IDictionary<ITeamBlockInfo, int> _teamBlockShiftCategoryPriority;

		public TeamBlockPriorityDefinitionInfo()
		{
			_teamBlockInfoPriorityList = new List<ITeamBlockInfoPriority>();
			_teamBlockShiftCategoryPriority = new Dictionary<ITeamBlockInfo, int>();
		}

		public void AddItem(ITeamBlockInfoPriority teamBlockInfoPriority, ITeamBlockInfo teamBlockInfo, int priority)
		{
			_teamBlockInfoPriorityList.Add(teamBlockInfoPriority);
			_teamBlockShiftCategoryPriority.Add(teamBlockInfo, priority);
		}

		public IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo
		{
			get { return (_teamBlockInfoPriorityList.OrderByDescending(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
		}

		public IList<ITeamBlockInfo> HighToLowShiftCategoryPriority()
		{
			var result = _teamBlockInfoPriorityList.OrderByDescending(s => s.ShiftCategoryPriority).Select(s => s.TeamBlockInfo).ToList();
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
