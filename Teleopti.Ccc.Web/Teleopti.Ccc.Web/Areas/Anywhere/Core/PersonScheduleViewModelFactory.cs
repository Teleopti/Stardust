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

		public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IAbsenceRepository absenceRepository, IPersonScheduleViewModelMapper personScheduleViewModelMapper)
		{
			_personRepository = personRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_absenceRepository = absenceRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
		}

		public PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var data = new PersonScheduleData
				{
					Person = _personRepository.Get(personId), 
					Date = date, 
					PersonScheduleDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), personId),
					Absences = _absenceRepository.LoadAllSortByName()
				};
			if (data.PersonScheduleDayReadModel != null && data.PersonScheduleDayReadModel.Shift != null)
				data.Shift = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(data.PersonScheduleDayReadModel.Shift);
			return _personScheduleViewModelMapper.Map(data);
		}
	}
}