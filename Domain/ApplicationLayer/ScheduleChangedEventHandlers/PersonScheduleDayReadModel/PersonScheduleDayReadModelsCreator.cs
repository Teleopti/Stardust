using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelsCreator : IPersonScheduleDayReadModelsCreator
	{
		private readonly IPersonRepository _personRepository;
		private readonly IJsonSerializer _serializer;

		public PersonScheduleDayReadModelsCreator(IPersonRepository personRepository, IJsonSerializer serializer)
		{
			_personRepository = personRepository;
			_serializer = serializer;
		}

		public IEnumerable<PersonScheduleDayReadModel> MakeReadModels(ProjectionChangedEventBase schedule)
		{
			var person = _personRepository.Load(schedule.PersonId);

			foreach (var scheduleDay in schedule.ScheduleDays)
			{
				var readModel = new PersonScheduleDayReadModel
					{
						PersonId = schedule.PersonId,
						TeamId = scheduleDay.TeamId,
						SiteId = scheduleDay.SiteId,
						BusinessUnitId = schedule.BusinessUnitId,
						Date = scheduleDay.Date
					};

				var layers = new List<SimpleLayer>();

				if (scheduleDay.Shift != null)
				{
					readModel.Start = scheduleDay.Shift.StartDateTime;
					readModel.End = scheduleDay.Shift.EndDateTime;

					var ls = from layer in scheduleDay.Shift.Layers
					         select new SimpleLayer
						         {
							         Color = ColorTranslator.ToHtml(Color.FromArgb(layer.DisplayColor)),
							         Description = layer.Name,
							         Start = layer.StartDateTime,
							         End = layer.EndDateTime,
							         Minutes = (int) layer.EndDateTime.Subtract(layer.StartDateTime).TotalMinutes,
							         IsAbsenceConfidential = layer.IsAbsenceConfidential,
									 ActivityId = layer.PayloadId
						         };

					layers.AddRange(ls);
				}

				if (scheduleDay.DayOff != null)
				{
					readModel.Start = scheduleDay.DayOff.StartDateTime;
					readModel.End = scheduleDay.DayOff.EndDateTime;
				}

				var model = new Model
					{
						Id = schedule.PersonId.ToString(),
						Date = scheduleDay.Date,
						FirstName = person.Name.FirstName,
						LastName = person.Name.LastName,
						EmploymentNumber = person.EmploymentNumber,
						Shift = new Shift
							{
								ContractTimeMinutes = (int) scheduleDay.ContractTime.TotalMinutes,
								WorkTimeMinutes = (int) scheduleDay.WorkTime.TotalMinutes,
								Projection = layers,
								IsFullDayAbsence = scheduleDay.IsFullDayAbsence
							},
						DayOff = scheduleDay.DayOff != null
							         ? new DayOff
								         {
									         Title = scheduleDay.Name,
									         Start = scheduleDay.DayOff.StartDateTime,
									         End = scheduleDay.DayOff.EndDateTime
								         }
							         : null
					};

				readModel.Model = _serializer.SerializeObject(model);

				yield return readModel;
			}
		}

	}
}