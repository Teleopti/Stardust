using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface IShiftCategoryPointInfo
	{
		ITeamBlockInfo TeamBlockInfo { get; }
		int Point { get; }
	}

	public class ShiftCategoryPointInfo : IShiftCategoryPointInfo
	{
		public ITeamBlockInfo TeamBlockInfo { get; private set; }
		public int Point { get; private set; }

		public ShiftCategoryPointInfo(ITeamBlockInfo teamInfo, int point)
		{
			TeamBlockInfo = teamInfo;
			Point = point;
		}
	}
}
