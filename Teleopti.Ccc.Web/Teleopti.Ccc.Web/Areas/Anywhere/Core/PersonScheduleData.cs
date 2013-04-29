﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
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
	}
}