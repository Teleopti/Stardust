using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ArchiveSchedulesModel
	{
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid JobResultId { get; set; }
		public List<Guid> SelectedTeams { get; set; }

		public ArchiveScheduleEvent CreateEvent(IEnumerable<IPerson> people)
		{
			return new ArchiveScheduleEvent(people.Select(person => person.Id.GetValueOrDefault()).ToArray())
			{
				StartDate = StartDate,
				EndDate = EndDate,
				FromScenario = FromScenario,
				ToScenario = ToScenario,
				JobResultId = JobResultId
			};
		}
	}
}