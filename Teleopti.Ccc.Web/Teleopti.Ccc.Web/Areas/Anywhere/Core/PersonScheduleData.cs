using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleData
	{
		public DateTime Date { get; set; }
		public IPerson Person { get; set; }
		public Model Model { get; set; }
		public IEnumerable<IActivity> Activities { get; set; }
		public IEnumerable<IAbsence> Absences { get; set; }
		public IEnumerable<IPersonAbsence> PersonAbsences { get; set; }
		public bool HasViewConfidentialPermission { get; set; }
		public ICommonNameDescriptionSetting CommonAgentNameSetting { get; set; }
	}
}