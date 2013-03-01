using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleDayReadModelRepository _personScheduleDayReadModelRepository;
		private readonly IPersonScheduleViewModelMapper _personScheduleViewModelMapper;

		public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository, IPersonScheduleViewModelMapper personScheduleViewModelMapper)
		{
			_personRepository = personRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
		}

		public PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var personData = new PersonScheduleData();
			personData.Person = _personRepository.Get(personId);
			personData.Date = date;

			personData.PersonScheduleDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), personId);
			
			return _personScheduleViewModelMapper.Map(personData);
		}
	}
}