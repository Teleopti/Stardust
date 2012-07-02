using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleDomainData
	{
		public static TimePeriod DefaultDisplayTime = new TimePeriod(8, 0, 17, 0);

		public DateOnly Date { get; set; }
		public Guid TeamId { get; set; }
		public IEnumerable<TeamScheduleDayDomainData> Days { get; set; }
		public DateTimePeriod DisplayTimePeriod { get; set; }
	}

	public class TeamScheduleDayDomainData
	{
		public bool HasDayOffUnder { get; set; }
		public DateTimePeriod DisplayTimePeriod { get; set; }
		public IPerson Person { get; set; }
		public ITeamScheduleProjection Projection { get; set; }
	}
}