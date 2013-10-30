using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

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
					select makeViewModel(s, model, shift, layers, data.UserTimeZone))
				.ToArray();
		}

		private static TeamScheduleShiftViewModel makeViewModel(PersonScheduleDayReadModel readModel, Model model, Shift shift, IEnumerable<TeamScheduleLayerViewModel> layers, TimeZoneInfo userTimeZone)
		{
			TeamScheduleDayOffViewModel dayOff = null;
			if (model.DayOff != null)
				dayOff = new TeamScheduleDayOffViewModel
					{
						DayOffName = model.DayOff.Title,
						Start = TimeZoneInfo.ConvertTimeFromUtc(model.DayOff.Start, userTimeZone).ToFixedDateTimeFormat(),
						Minutes = (int) model.DayOff.End.Subtract(model.DayOff.Start).TotalMinutes
					};
			return new TeamScheduleShiftViewModel
				{
					ContractTimeMinutes = shift.ContractTimeMinutes,
					PersonId = readModel.PersonId.ToString(),
					Projection = layers,
					IsFullDayAbsence = model.Shift != null && model.Shift.IsFullDayAbsence,
					DayOff = dayOff
				};
		}

		private static IEnumerable<TeamScheduleLayerViewModel> mapLayers(TimeZoneInfo userTimeZone, Shift shift, bool canSeeConfidentialAbsence)
		{
			var layers = shift.Projection ?? new SimpleLayer[] {};
			return (
					   from l in layers
					   let canSeeLayerInfo = CanSeeLayerInfo(canSeeConfidentialAbsence, l)
					   let color = canSeeLayerInfo ? l.Color : ConfidentialPayloadValues.DisplayColor.ToHtml()
					   let description = canSeeLayerInfo ? l.Description : ConfidentialPayloadValues.Description.Name
					   let start = TimeZoneInfo.ConvertTimeFromUtc(l.Start, userTimeZone)
				       select new TeamScheduleLayerViewModel
					       {
						       Color = color,
							   Description = description,
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