using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleViewModelMapper : IGroupScheduleViewModelMapper
	{
		public IEnumerable<GroupScheduleShiftViewModel> Map(GroupScheduleData data)
		{
			var canSeePersons = data.CanSeePersons.ToArray();
			var canSeeConfidentialAbsencesFor = data.CanSeeConfidentialAbsencesFor ?? new IPerson[] { };
			var published = new PublishedScheduleSpecification(canSeePersons, data.Date);

			foreach (var person in canSeePersons)
			{
				var personScheduleDayReadModel = data.Schedules.FirstOrDefault(s => s.PersonId.Equals(person.Id.Value));
				if (personScheduleDayReadModel != null && (data.CanSeeUnpublishedSchedules || published.IsSatisfiedBy(personScheduleDayReadModel)))
				{
					var model = JsonConvert.DeserializeObject<Model>(personScheduleDayReadModel.Model ?? "{}");
					var shift = model.Shift ?? new Shift();
					var canSeeConfidentialAbsence = canSeeConfidentialAbsencesFor.Any(x => x.Id == personScheduleDayReadModel.PersonId);
					var layers = mapLayers(data.UserTimeZone, shift, canSeeConfidentialAbsence);
					yield return makeViewModel(personScheduleDayReadModel.PersonId, model, shift, layers, data.UserTimeZone);
				}
				else
				{
					yield return new GroupScheduleShiftViewModel
					{
						PersonId = person.Id.Value.ToString(),
						Name = person.Name.FirstName + " " + person.Name.LastName,
						IsFullDayAbsence = false,
					};
				}
			}
		}

		private static GroupScheduleShiftViewModel makeViewModel(Guid personId, Model model, Shift shift, IEnumerable<GroupScheduleLayerViewModel> layers, TimeZoneInfo userTimeZone)
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
					PersonId = personId.ToString(),
					Name = model.FirstName + " " + model.LastName,
					Projection = layers,
					IsFullDayAbsence = model.Shift != null && model.Shift.IsFullDayAbsence,
					DayOff = dayOff
				};
		}

		private static IEnumerable<GroupScheduleLayerViewModel> mapLayers(TimeZoneInfo userTimeZone, Shift shift, bool canSeeConfidentialAbsence)
		{
			var layers = shift.Projection ?? new SimpleLayer[] { };
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