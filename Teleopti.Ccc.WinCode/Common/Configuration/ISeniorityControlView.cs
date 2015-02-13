
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public interface ISeniorityControlView
	{
		void RefreshListBoxWorkingDays(int newIndex);
		void RefreshListBoxShiftCategoryRank(int newIndex);
		void SetChangedInfo(ISeniorityWorkDayRanks workDayRanks);
	}
}
