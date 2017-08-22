using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleViewModelMapper : IGroupScheduleViewModelMapper
	{
		public IEnumerable<GroupScheduleShiftViewModel> Map(GroupScheduleData data)
		{
			var canSeePersons = data.CanSeePersons.ToLookup(p => p.Id.GetValueOrDefault());
			var schedulesForPersons = data.Schedules.ToLookup(s => s.PersonId);

			var schedulesWithPersons = from s in schedulesForPersons
						   let person = canSeePersons[s.Key].SingleOrDefault()
						   where person != null
						   select new
						   {
							   person,
							   schedule = s.FirstOrDefault()
						   };

			var personsWithoutSchedules = from p in canSeePersons
						      let schedules = schedulesForPersons[p.Key]
						      where !schedules.Any()
						      select new
						      {
							      person = p.FirstOrDefault(),
							      schedule = (PersonScheduleDayReadModel)null
						      };

			var personsAndSchedules = schedulesWithPersons.Concat(personsWithoutSchedules).ToArray();

			var canSeeConfidentialAbsencesFor = data.CanSeeConfidentialAbsencesFor ?? new IPerson[] { };
			var published = new PublishedScheduleSpecification(data.CanSeePersons,
			                                                   new DateOnly(TimeZoneHelper.ConvertFromUtc(data.Date, data.UserTimeZone)));

			return (from item in personsAndSchedules
				let displaySchedule = data.CanSeeUnpublishedSchedules ||
						      (
							      item.schedule != null && published.IsSatisfiedBy(item.schedule)
						      )
				let schedule = displaySchedule ? item.schedule : null
				let model = JsonConvert.DeserializeObject<Model>(schedule?.Model ?? "{}")
				let shift = model.Shift ?? new Shift()
				let canSeeConfidentialAbsence = canSeeConfidentialAbsencesFor.Any(x => schedule != null && x.Id == schedule.PersonId)
				let layers = mapLayers(data.UserTimeZone, shift, canSeeConfidentialAbsence)
				select makeViewModel(item.person, schedule, model, shift, layers, data.UserTimeZone, data.CommonAgentNameSetting))
				.ToArray();
		}

		private static GroupScheduleShiftViewModel makeViewModel(IPerson person, IPersonScheduleDayReadModel readModel, Model model, Shift shift, IEnumerable<GroupScheduleProjectionViewModel> layers, TimeZoneInfo userTimeZone, ICommonNameDescriptionSetting commonNameDescriptionSetting)
		{
			GroupScheduleDayOffViewModel dayOff = null;
			if (model.DayOff != null)
				dayOff = new GroupScheduleDayOffViewModel
				{
					DayOffName = model.DayOff.Title,
					Start = TimeZoneInfo.ConvertTimeFromUtc(model.DayOff.Start, userTimeZone).ToFixedDateTimeFormat(),
					Minutes = (int)model.DayOff.End.Subtract(model.DayOff.Start).TotalMinutes
				};
			return new GroupScheduleShiftViewModel
			{
				ContractTimeMinutes = shift.ContractTimeMinutes,
				PersonId = person.Id.ToString(),
				Date = readModel == null ? "" : readModel.Date.ToFixedDateFormat(),
				Name = commonNameDescriptionSetting.BuildCommonNameDescription(person),
				Projection = layers,
				IsFullDayAbsence = model.Shift != null && model.Shift.IsFullDayAbsence,
				DayOff = dayOff
			};
		}

		private static IEnumerable<GroupScheduleProjectionViewModel> mapLayers(TimeZoneInfo userTimeZone, Shift shift, bool canSeeConfidentialAbsence)
		{
			var layers = shift.Projection ?? new SimpleLayer[] { };

			if (shift.IsFullDayAbsence && layers.Count > 1)
			{
				var mergedLayer = new SimpleLayer
				{
					Start = layers[0].Start,
					End = layers[layers.Count - 1].End,
					Minutes = (int) layers[layers.Count - 1].End.Subtract(layers[0].Start).TotalMinutes,
					Color = layers[0].Color,
					Description = layers[0].Description,
					IsAbsenceConfidential = layers[0].IsAbsenceConfidential
				};
				layers = new[] {mergedLayer};
			}

			return (
				from l in layers
				let canSeeLayerInfo = CanSeeLayerInfo(canSeeConfidentialAbsence, l)
				let color = canSeeLayerInfo ? l.Color : ConfidentialPayloadValues.DisplayColorHex
				let description = canSeeLayerInfo ? l.Description : ConfidentialPayloadValues.Description.Name
				let start = TimeZoneInfo.ConvertTimeFromUtc(l.Start, userTimeZone)
				select new GroupScheduleProjectionViewModel
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