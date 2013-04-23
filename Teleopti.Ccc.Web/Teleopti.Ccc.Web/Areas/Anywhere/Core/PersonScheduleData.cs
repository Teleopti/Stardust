using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
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
		public IEnumerable<IAbsence> Absences { get; set; }
		public IEnumerable<IPersonAbsence> PersonAbsences { get; set; }
	}
}