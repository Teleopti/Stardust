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

		public ArchiveScheduleEvent CreateArchiveEvent(IEnumerable<IPerson> people)
		{
			return new ArchiveScheduleEvent(people.Select(person => person.Id.GetValueOrDefault()).ToArray())
			{
				StartDate = new DateOnly(StartDate),
				EndDate = new DateOnly(EndDate),
				FromScenario = FromScenario,
				ToScenario = ToScenario,
				JobResultId = JobResultId
			};
		}

		public ImportScheduleEvent CreateImportEvent(IEnumerable<IPerson> people)
		{
			return new ImportScheduleEvent(people.Select(person => person.Id.GetValueOrDefault()).ToArray())
			{
				StartDate = new DateOnly(StartDate),
				EndDate = new DateOnly(EndDate),
				FromScenario = FromScenario,
				ToScenario = ToScenario,
				JobResultId = JobResultId
			};
		}
	}
}