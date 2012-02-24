using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDayDomainData : IScheduleColorSource
	{
		public DateOnly Date { get; set; }

		public IEnumerable<IPersonRequest> PersonRequests { get; set; }
		public IScheduleDay ScheduleDay { get; set; }
		public IVisualLayerCollection Projection { get; set; }

		public IPreferenceDay PreferenceDay { get { return null; } } //only for IScheduleColorSource. Need to rethink this..
	}
}