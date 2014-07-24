using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleData
	{
		public GroupScheduleData()
		{
			CanSeeUnpublishedSchedules = true;
		}

		public TimeZoneInfo UserTimeZone { get; set; }
		public DateTime Date { get; set; }
		public IEnumerable<PersonScheduleDayReadModel> Schedules { get; set; }
		public bool CanSeeUnpublishedSchedules { get; set; }
		public IEnumerable<IPerson> CanSeeConfidentialAbsencesFor { get; set; }
		public IEnumerable<IPerson> CanSeePersons { get; set; }
		public ICommonNameDescriptionSetting CommonAgentNameSetting { get; set; }
	}
}