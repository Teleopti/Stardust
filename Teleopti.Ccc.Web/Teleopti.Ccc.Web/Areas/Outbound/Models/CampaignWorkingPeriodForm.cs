using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
	public class CampaignWorkingPeriodForm
	{
		public Guid? Id;
		public TimeSpan StartTime;
		public TimeSpan EndTime;
		public Guid? CampaignId;
	}

	public class CampaignWorkingPeriodAssignmentForm
	{
		public DayOfWeek WeekDay;
		public IList<Guid> CampaignWorkingPeriods;
		public Guid? CampaignId;
	}

}