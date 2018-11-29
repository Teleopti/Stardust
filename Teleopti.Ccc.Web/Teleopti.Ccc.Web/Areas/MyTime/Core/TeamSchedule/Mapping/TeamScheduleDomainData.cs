using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleDomainData
	{
		public static TimePeriod DefaultDisplayTime = new TimePeriod(8, 0, 17, 0);

		public DateOnly Date { get; set; }
		public Guid TeamOrGroupId { get; set; }
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

	public class TeamScheduleViewModelData
	{
		public DateOnly ScheduleDate { get; set; }

		public IList<Guid> TeamIdList { get; set; }

		public Paging Paging { get; set; }

		public TimeFilterInfo TimeFilter { get; set; }

		public string SearchNameText { get; set; }

		public string TimeSortOrder { get; set; }
	}
}