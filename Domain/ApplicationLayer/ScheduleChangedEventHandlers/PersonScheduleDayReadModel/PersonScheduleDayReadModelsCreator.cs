using System.Collections.Generic;
using System.Drawing;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IEnumerable<PersonScheduleDayReadModel> GetReadModels(ProjectionChangedEventBase schedule)
		{
			var person = _personRepository.Load(schedule.PersonId);

			foreach (var scheduleDay in schedule.ScheduleDays)
			{
				if (scheduleDay.Layers.Count == 0) continue;

				var ret = new PersonScheduleDayReadModel();

				ret.PersonId = schedule.PersonId;
				ret.TeamId = scheduleDay.TeamId;
				ret.SiteId = scheduleDay.SiteId;
				ret.BusinessUnitId = schedule.BusinessUnitId;
				ret.Date = scheduleDay.Date;

				if (scheduleDay.StartDateTime.HasValue && scheduleDay.EndDateTime.HasValue)
				{
					ret.ShiftStart = scheduleDay.StartDateTime;
					ret.ShiftEnd = scheduleDay.EndDateTime;
				}

				var shift = new Shift
				{
					Date = scheduleDay.Date,
					FirstName = person.Name.FirstName,
					LastName = person.Name.LastName,
					EmploymentNumber = person.EmploymentNumber,
					Id = schedule.PersonId.ToString(),
					ContractTimeMinutes = (int)scheduleDay.ContractTime.TotalMinutes,
					WorkTimeMinutes = (int)scheduleDay.WorkTime.TotalMinutes,
					Projection = new List<SimpleLayer>()
				};

				foreach (var layer in scheduleDay.Layers)
				{
					shift.Projection.Add(new SimpleLayer
					{
						Color = ColorTranslator.ToHtml(Color.FromArgb(layer.DisplayColor)),
						Title = layer.Name,
						Start = layer.StartDateTime,
						End = layer.EndDateTime,
						Minutes = (int)layer.EndDateTime.Subtract(layer.StartDateTime).TotalMinutes
					});
				}

				ret.Shift = _serializer.SerializeObject(shift);

				yield return ret;
			}
		}
	}
}