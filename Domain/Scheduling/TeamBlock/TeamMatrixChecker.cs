using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamMatrixChecker
	{
		public IEnumerable<ITeamInfo> CheckTeamList(IEnumerable<ITeamInfo> teamList, DateOnlyPeriod period)
		{
			var okList = new List<ITeamInfo>();
			foreach (var teamInfo in teamList)
			{
				bool banned = false;
				foreach (var member in teamInfo.GroupMembers)
				{
					if (!teamInfo.MatrixesForMemberAndPeriod(member, period).Any())
					{
						banned = true;
						break;
					}
				}
				if (!banned)
					okList.Add(teamInfo);
			}

			return okList;
		}
	}
}