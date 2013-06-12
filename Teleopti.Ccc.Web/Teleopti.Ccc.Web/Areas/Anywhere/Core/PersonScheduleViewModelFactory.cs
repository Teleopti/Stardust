using System;
using System.Dynamic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IPersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IJsonDeserializer<ExpandoObject> _deserializer;

		public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IAbsenceRepository absenceRepository, IPersonScheduleViewModelMapper personScheduleViewModelMapper, IPersonAbsenceRepository personAbsenceRepository, IJsonDeserializer<ExpandoObject> deserializer)
		{
			_personRepository = personRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_absenceRepository = absenceRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
			_personAbsenceRepository = personAbsenceRepository;
			_deserializer = deserializer;
		}

		public PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var person = _personRepository.Get(personId);
			var personScheduleDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), personId);
			
			var absencePeriodStartTime = TimeZoneInfo.ConvertTimeToUtc(date, person.PermissionInformation.DefaultTimeZone());
			var absencePeriod = new DateTimePeriod(absencePeriodStartTime, absencePeriodStartTime.AddHours(24));

			var data = new PersonScheduleData
				{
					Person = person,
					Date = date,
					Absences = _absenceRepository.LoadAllSortByName(),
					PersonAbsences = _personAbsenceRepository.Find(new[] { person }, absencePeriod)
				};

			if (personScheduleDayReadModel != null && personScheduleDayReadModel.Shift != null)
				data.Shift = _deserializer.DeserializeObject(personScheduleDayReadModel.Shift);

			return _personScheduleViewModelMapper.Map(data);
		}
	}
}