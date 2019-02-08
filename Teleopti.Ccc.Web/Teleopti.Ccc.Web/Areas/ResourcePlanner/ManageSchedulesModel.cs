using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ManageSchedulesModel
	{
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public Guid JobResultId { get; set; }
		public List<Guid> SelectedTeams { get; set; }

		public T CreateImportEvent<T>(IEnumerable<IPerson> people) where T: ManageScheduleBaseEvent, new()
		{
			var @event = new T
			{
				StartDate = StartDate,
				EndDate = EndDate,
				FromScenario = FromScenario,
				ToScenario = ToScenario,
				JobResultId = JobResultId
			};
			@event.PersonIds = @event.PersonIds.Concat(people.Select(person => person.Id.GetValueOrDefault())).ToArray();
			return @event;
		}
	}
}