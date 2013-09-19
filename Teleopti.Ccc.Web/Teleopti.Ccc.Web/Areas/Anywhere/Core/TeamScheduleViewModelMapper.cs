using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleViewModelMapper : ITeamScheduleViewModelMapper
	{
		public IEnumerable<Shift> Map(TeamScheduleData data)
		{
			var published = new PublishedScheduleSpecification(data.CanSeePersons, data.Date);
			return (from s in data.Schedules
			        where
				        data.CanSeeUnpublishedSchedules ||
				        published.IsSatisfiedBy(s)
			        let shift = JsonConvert.DeserializeObject<Shift>(s.Shift)
			        let mutated = MutateConfidentialAbsenceLayers(s, shift, data)
			        select mutated)
				.ToArray();
		}

		private Shift MutateConfidentialAbsenceLayers(PersonScheduleDayReadModel readModel, Shift shift, TeamScheduleData data)
		{
			var canSeeConfidentialAbsence = data.CanSeeConfidentialAbsencesFor.Any(x => x.Id == readModel.PersonId);

			if (canSeeConfidentialAbsence)
				return shift;

			var confidentialAbsenceLayers = shift.Projection.Where(l => l.IsAbsenceConfidential); 
			confidentialAbsenceLayers.ForEach(l =>
				{
					l.Color = ConfidentialPayloadValues.DisplayColor.ToHtml();
					l.Title = ConfidentialPayloadValues.Description.Name;
				});

			return shift;
		}
	}
}