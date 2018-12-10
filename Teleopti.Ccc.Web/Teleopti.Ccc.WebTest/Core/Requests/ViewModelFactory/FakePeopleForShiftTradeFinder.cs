using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;


namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	public class FakePeopleForShiftTradeFinder : IPeopleForShiftTradeFinder
	{
		private IList<IPersonAuthorization> storage = new List<IPersonAuthorization>();
		private IPersonRepository _personRepository;

		public FakePeopleForShiftTradeFinder(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public void Has(IPersonAuthorization authorization)
		{
			storage.Add(authorization);
		}

		public IList<IPersonAuthorization> GetPeople(IPerson personFrom, DateOnly shiftTradeDate, IList<Guid> teamIdList, string name,
			NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName)
		{
			if (string.IsNullOrEmpty(name)) return storage;
			
			var results = from s in storage
				let person = _personRepository.Get(s.PersonId)
				where person != null && (person.Name.LastName + person.Name.FirstName).Contains(name)
				select s;
			return results.ToList();

		}

		public IList<IPersonAuthorization> GetPeople(IPerson personFrom, DateOnly shiftTradeDate, IList<Guid> peopleIdList)
		{
			return storage;
		}
	}
}