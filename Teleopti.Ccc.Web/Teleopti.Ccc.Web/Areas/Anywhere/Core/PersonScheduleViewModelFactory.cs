using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleViewModelMapper _personScheduleViewModelMapper;

		public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleViewModelMapper personScheduleViewModelMapper)
		{
			_personRepository = personRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
		}

		public PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var personData = new PersonScheduleData();
			personData.Person = _personRepository.Get(personId);
			personData.Date = date;
			return _personScheduleViewModelMapper.Map(personData);
		}
	}
}