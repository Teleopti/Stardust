using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleViewModelMapper : IGroupScheduleViewModelMapper
	{
		public IEnumerable<GroupScheduleShiftViewModel> Map(GroupScheduleData data)
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

		private static GroupScheduleShiftViewModel makeViewModel(PersonScheduleDayReadModel readModel, Model model, Shift shift, IEnumerable<GroupScheduleLayerViewModel> layers, TimeZoneInfo userTimeZone)
		{
			GroupScheduleDayOffViewModel dayOff = null;
			if (model.DayOff != null)
				dayOff = new GroupScheduleDayOffViewModel
					{
						DayOffName = model.DayOff.Title,
						Start = TimeZoneInfo.ConvertTimeFromUtc(model.DayOff.Start, userTimeZone).ToFixedDateTimeFormat(),
						Minutes = (int) model.DayOff.End.Subtract(model.DayOff.Start).TotalMinutes
					};
			return new GroupScheduleShiftViewModel
				{
					ContractTimeMinutes = shift.ContractTimeMinutes,
					PersonId = readModel.PersonId.ToString(),
					Projection = layers,
					IsFullDayAbsence = model.Shift != null && model.Shift.IsFullDayAbsence,
					DayOff = dayOff
				};
		}

		private static IEnumerable<GroupScheduleLayerViewModel> mapLayers(TimeZoneInfo userTimeZone, Shift shift, bool canSeeConfidentialAbsence)
		{
			var layers = shift.Projection ?? new SimpleLayer[] {};
			return (
					   from l in layers
					   let canSeeLayerInfo = CanSeeLayerInfo(canSeeConfidentialAbsence, l)
					   let color = canSeeLayerInfo ? l.Color : ConfidentialPayloadValues.DisplayColorHex
					   let description = canSeeLayerInfo ? l.Description : ConfidentialPayloadValues.Description.Name
					   let start = TimeZoneInfo.ConvertTimeFromUtc(l.Start, userTimeZone)
				       select new GroupScheduleLayerViewModel
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