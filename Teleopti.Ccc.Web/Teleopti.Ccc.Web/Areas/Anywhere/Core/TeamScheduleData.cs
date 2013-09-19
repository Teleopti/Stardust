﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleData
	{
		public TeamScheduleData()
		{
			CanSeeUnpublishedSchedules = true;
		}

		public IPerson User { get; set; }
		public DateTime Date { get; set; }
		public IEnumerable<PersonScheduleDayReadModel> Schedules { get; set; }
		public bool CanSeeUnpublishedSchedules { get; set; }
		public IEnumerable<IPerson> CanSeeConfidentialAbsencesFor { get; set; }
		public IEnumerable<IPerson> CanSeePersons { get; set; }
	}
}