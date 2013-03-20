using System;
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
	}
}