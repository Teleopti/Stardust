using System;
using System.Collections;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleData
	{
		public DateTime Date { get; set; }
		public IPerson Person { get; set; }
		public PersonScheduleDayReadModel PersonScheduleDayReadModel { get; set; }
		public dynamic Shift { get; set; }
		public IEnumerable Absences { get; set; }
	}
}