using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDomainData
	{
		public DateOnly Date { get; set; }
		public IEnumerable<WeekScheduleDayDomainData> Days { get; set; }
	}
}