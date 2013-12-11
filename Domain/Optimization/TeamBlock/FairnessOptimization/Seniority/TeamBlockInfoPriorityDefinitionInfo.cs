using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        //IEnumerable<double> HighToLowSeniorityList { get; }
		//IEnumerable<int> HighToLowShiftCategoryPriorityList { get; }
		//IEnumerable<int> LowToHighShiftCategoryPriorityList { get; }
        //IEnumerable<double> LowToHighSeniorityList { get; }
        //ITeamBlockInfo BlockOnSeniority(double seniority);
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

		//public IEnumerable<double> HighToLowSeniorityList
		//{
		//	get { return (_teamBlockInfoPriorityList.Select(s => s.Seniority)).ToList().OrderByDescending(s => s); }
		//}

		public IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo
		{
			get { return (_teamBlockInfoPriorityList.OrderByDescending(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
		}

		public IList<ITeamBlockInfo> LowToHighSeniorityListBlockInfo
		{
			get { return (_teamBlockInfoPriorityList.OrderBy(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
		}

		//public IEnumerable<int> HighToLowShiftCategoryPriorityList
		//{
		//	get { return (_teamBlockInfoPriorityList.Select(s => s.ShiftCategoryPriority)).ToList().OrderByDescending(s => s); }
		//}

		//public IEnumerable<int> LowToHighShiftCategoryPriorityList
		//{
		//	get { return (_teamBlockInfoPriorityList.Select(s => s.ShiftCategoryPriority)).ToList().OrderBy(s => s); }
		//}

		//public IEnumerable<double> LowToHighSeniorityList
		//{
		//	get { return (_teamBlockInfoPriorityList.Select(s => s.Seniority)).ToList().OrderBy(s => s); }
		//}


		//public ITeamBlockInfo BlockOnSeniority(double seniority)
		//{
		//	return _teamBlockInfoPriorityList.FirstOrDefault(s => s.Seniority == seniority).TeamBlockInfo;
		//}

		public int GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
		{
			return _teamBlockInfoPriorityList.FirstOrDefault(s => s.TeamBlockInfo == teamBlockInfo).ShiftCategoryPriority;
		}

		public void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, int shiftCategoryPriority)
		{
			var info = _teamBlockInfoPriorityList.FirstOrDefault(s => s.TeamBlockInfo == teamBlockInfo);
			if (info == null) return;
			info.ShiftCategoryPriority = shiftCategoryPriority;
		}
	}
}
