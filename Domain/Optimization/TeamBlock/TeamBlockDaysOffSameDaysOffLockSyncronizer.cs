using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockDaysOffSameDaysOffLockSyncronizer
	{
		public void SyncLocks(DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences, IEnumerable<ITeamInfo> allTeamInfoListOnStartDate)
		{
			if (optimizationPreferences.Extra.UseTeamSameDaysOff)
			{
				foreach (var teamInfo in allTeamInfoListOnStartDate)
				{
					foreach (var dateOnly in selectedPeriod.DayCollection())
					{
						foreach (var inspectedMatrix in teamInfo.MatrixesForGroupAndDate(dateOnly))
						{
							if (inspectedMatrix.IsDayLocked(dateOnly))
							{
								foreach (var otherMatrixPro in teamInfo.MatrixesForGroupAndDate(dateOnly))
								{
									otherMatrixPro.LockDay(dateOnly);
								}
							}
						}
					}
				}
			}
		}
	}
}