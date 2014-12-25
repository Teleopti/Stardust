using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport
{
	public class LeaderboardQuery
	{
		public DateOnly Date { get; set; }
		public LeadboardQueryType Type { get; set; }
		public Guid SelectedId { get; set; }
	}
	public enum LeadboardQueryType
	{
		MyOwn,
		Team,
		Site,
		Everyone  //relative
	}
}