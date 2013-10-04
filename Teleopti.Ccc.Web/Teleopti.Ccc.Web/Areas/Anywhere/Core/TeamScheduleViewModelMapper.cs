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
			        let model = JsonConvert.DeserializeObject<Model>(s.Model ?? "{}")
					let shift = model.Shift ?? new Shift()
					let canSeeConfidentialAbsence = data.CanSeeConfidentialAbsencesFor.Any(x => x.Id == s.PersonId)
					let layers = mapLayers(data.UserTimeZone, shift, canSeeConfidentialAbsence)
					select makeViewModel(s, model, shift, layers))
				.ToArray();
		}

		private static TeamScheduleShiftViewModel makeViewModel(PersonScheduleDayReadModel readModel, Model model, Shift shift, IEnumerable<TeamScheduleLayerViewModel> layers)
		{
			return new TeamScheduleShiftViewModel
				{
					ContractTimeMinutes = shift.ContractTimeMinutes,
					FirstName = model.FirstName,
					Id = readModel.PersonId.ToString(),
					LastName = model.LastName,
					Projection = layers,
					IsDayOff = model.DayOff != null,
					IsFullDayAbsence = model.Shift.IsFullDayAbsence
				};
		}

		private static IEnumerable<TeamScheduleLayerViewModel> mapLayers(TimeZoneInfo userTimeZone, Shift shift, bool canSeeConfidentialAbsence)
		{
			var layers = shift.Projection ?? new SimpleLayer[] {};
			return (
					   from l in layers
					   let canSeeLayerInfo = CanSeeLayerInfo(canSeeConfidentialAbsence, l)
					   let color = canSeeLayerInfo ? l.Color : ConfidentialPayloadValues.DisplayColor.ToHtml()
					   let title = canSeeLayerInfo ? l.Title : ConfidentialPayloadValues.Description.Name
					   let start = TimeZoneInfo.ConvertTimeFromUtc(l.Start, userTimeZone)
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