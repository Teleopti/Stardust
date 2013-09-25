using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleViewModelMapper : ITeamScheduleViewModelMapper
	{
		public IEnumerable<TeamScheduleShiftViewModel> Map(TeamScheduleData data)
		{
			var published = new PublishedScheduleSpecification(data.CanSeePersons, data.Date);
			return (from s in data.Schedules
			        where
				        data.CanSeeUnpublishedSchedules ||
				        published.IsSatisfiedBy(s)
			        let shift = JsonConvert.DeserializeObject<Shift>(s.Shift)
					let canSeeConfidentialAbsence = data.CanSeeConfidentialAbsencesFor.Any(x => x.Id == s.PersonId)
					let layers = mapLayers(data.User, shift, canSeeConfidentialAbsence)
			        select makeViewModel(shift, layers))
				.ToArray();
		}

		private static TeamScheduleShiftViewModel makeViewModel(Shift shift, IEnumerable<TeamScheduleLayerViewModel> layers)
		{
			return new TeamScheduleShiftViewModel
				{
					ContractTimeMinutes = shift.ContractTimeMinutes,
					FirstName = shift.FirstName,
					Id = shift.Id,
					LastName = shift.LastName,
					Projection = layers
				};
		}

		private static IEnumerable<TeamScheduleLayerViewModel> mapLayers(IPerson user, Shift shift, bool canSeeConfidentialAbsence)
		{
			return (
				       from l in shift.Projection
					   let canSeeLayerInfo = CanSeeLayerInfo(canSeeConfidentialAbsence, l)
					   let color = canSeeLayerInfo ? l.Color : ConfidentialPayloadValues.DisplayColor.ToHtml()
					   let title = canSeeLayerInfo ? l.Title : ConfidentialPayloadValues.Description.Name
					   let start = TimeZoneInfo.ConvertTimeFromUtc(l.Start, user.PermissionInformation.DefaultTimeZone())
				       select new TeamScheduleLayerViewModel
					       {
						       Color = color,
							   Title = title,
							   Start = start.ToFixedDateTimeFormat(),
						       Minutes = l.Minutes
					       })
				.ToArray();
		}

		private static bool CanSeeLayerInfo(bool canSeeConfidentialAbsence, SimpleLayer layer)
		{
			if (layer.IsAbsenceConfidential)
				return canSeeConfidentialAbsence;
			return true;
		}
	}
}