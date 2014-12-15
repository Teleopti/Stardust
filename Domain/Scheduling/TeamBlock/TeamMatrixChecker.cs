using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamMatrixChecker
	{
		TeamMatrixCheckerResult CheckTeamList(IEnumerable<ITeamInfo> teamList, DateOnlyPeriod period);
	}

	public class TeamMatrixChecker : ITeamMatrixChecker
	{
		public TeamMatrixCheckerResult CheckTeamList(IEnumerable<ITeamInfo> teamList, DateOnlyPeriod period)
		{
			var okList = new List<ITeamInfo>();
			var bannedList = new List<ITeamInfo>();
			foreach (var teamInfo in teamList)
			{
				bool banned = false;
				foreach (var member in teamInfo.GroupMembers)
				{
					if (!teamInfo.MatrixesForMemberAndPeriod(member, period).Any())
					{
						bannedList.Add(teamInfo);
						banned = true;
						break;
					}
				}
				if (!banned)
					okList.Add(teamInfo);
			}

			return new TeamMatrixCheckerResult(okList, bannedList);
		}
	}

	public class TeamMatrixCheckerResult
	{
		private readonly IEnumerable<ITeamInfo> _okList;
		private readonly IEnumerable<ITeamInfo> _bannedList;

		public TeamMatrixCheckerResult(IEnumerable<ITeamInfo> okList, IEnumerable<ITeamInfo> bannedList)
		{
			_okList = okList;
			_bannedList = bannedList;
		}

		public IEnumerable<ITeamInfo> OkList
		{
			get { return _okList; }
		}

		public IEnumerable<ITeamInfo> BannedList
		{
			get { return _bannedList; }
		}
	}
}