using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockLockValidator
	{
		bool ValidateLocks(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);	
	}

	public class TeamBlockLockValidator : ITeamBlockLockValidator
	{
		public bool ValidateLocks(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			var period1 = teamBlockInfo1.BlockInfo.BlockPeriod;
			var period2 = teamBlockInfo2.BlockInfo.BlockPeriod;
			var scheduleMatrixPros1 = teamBlockInfo1.TeamInfo.MatrixesForGroupAndPeriod(period1);
			var scheduleMatrixPros2 = teamBlockInfo2.TeamInfo.MatrixesForGroupAndPeriod(period2);
			
			IList<IScheduleDayPro> unlockedDays1 = scheduleMatrixPros1.SelectMany(scheduleMatrixPro => scheduleMatrixPro.UnlockedDays).ToList();
			IList<IScheduleDayPro> unlockedDays2 = scheduleMatrixPros2.SelectMany(scheduleMatrixPro => scheduleMatrixPro.UnlockedDays).ToList();

			foreach (var dateOnly in period1.DayCollection())
			{
				var dateIsUnlocked = unlockedDays1.Any(scheduleDayPro => scheduleDayPro.Day.Equals(dateOnly));
				if (dateIsUnlocked == false) return false;
			}

			foreach (var dateOnly in period2.DayCollection())
			{
				var dateIsUnlocked = unlockedDays2.Any(scheduleDayPro => scheduleDayPro.Day.Equals(dateOnly));
				if (dateIsUnlocked == false) return false;
			}

			return true;
		}
	}
}
