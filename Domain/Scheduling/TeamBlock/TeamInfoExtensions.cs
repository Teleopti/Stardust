using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public static class TeamInfoExtensions
	{
		public static void LockDays(this ITeamInfo teamInfo, DateOnly date)
		{
			foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroupAndDate(date))
			{
				scheduleMatrixPro.LockDay(date);
			}
		}

		public static void LockDays(this ITeamInfo teamInfo, IEnumerable<DateOnly> dates)
		{
			foreach (var date in dates)
			{
				LockDays(teamInfo, date);
			}
		}
	}
}