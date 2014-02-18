using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockPriorityDefinitionInfo
    {
        double GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo);
        void AddItem(ITeamBlockInfoPriority teamBlockInfoPriority, ITeamBlockInfo teamBlockInfo, double priority);
        void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, double shiftCategoryPriority);
	    IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo { get; }
	    IList<ITeamBlockInfo> HighToLowShiftCategoryPriority();
    }

	public class TeamBlockPriorityDefinitionInfo : ITeamBlockPriorityDefinitionInfo
	{
		private readonly IList<ITeamBlockInfoPriority> _teamBlockInfoPriorityList;
        private readonly IDictionary<ITeamBlockInfo, double> _teamBlockShiftCategoryPriority;

		public TeamBlockPriorityDefinitionInfo()
		{
			_teamBlockInfoPriorityList = new List<ITeamBlockInfoPriority>();
			_teamBlockShiftCategoryPriority = new Dictionary<ITeamBlockInfo, double>();
		}

        public void AddItem(ITeamBlockInfoPriority teamBlockInfoPriority, ITeamBlockInfo teamBlockInfo, double priority)
		{
			_teamBlockInfoPriorityList.Add(teamBlockInfoPriority);
			_teamBlockShiftCategoryPriority.Add(teamBlockInfo, priority);
		}

        /// <summary>
        /// The seniority is based on ranking. The lowest the rank is the higest the priority is.
        /// In this case the higest rank is 0
        /// </summary>
		public IList<ITeamBlockInfo> HighToLowSeniorityListBlockInfo
		{
			get { return (_teamBlockInfoPriorityList.OrderBy(s => s.Seniority).Select(s => s.TeamBlockInfo).ToList()); }
		}

		public IList<ITeamBlockInfo> HighToLowShiftCategoryPriority()
		{
			var result = _teamBlockInfoPriorityList.OrderByDescending(s => s.ShiftCategoryPriority).Select(s => s.TeamBlockInfo).ToList();
			return result;
		}

        public double GetShiftCategoryPriorityOfBlock(ITeamBlockInfo teamBlockInfo)
		{
			return _teamBlockShiftCategoryPriority[teamBlockInfo];
		}

        public void SetShiftCategoryPoint(ITeamBlockInfo teamBlockInfo, double shiftCategoryPriority)
		{
			_teamBlockShiftCategoryPriority[teamBlockInfo] = shiftCategoryPriority;
		}
	}
}
