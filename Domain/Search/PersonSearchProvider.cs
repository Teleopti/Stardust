using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Search
{
	public class PersonSearchProvider
	{
		private readonly IPersonRepository _personRepository;

		public PersonSearchProvider(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public virtual IList<IPerson> FindPersonsByKeywords(string keywords)
		{
			var separateKeywords = keywords.Split(' ');
			var persons = _personRepository.FindPersonsByKeywords(separateKeywords);

			return persons;
		}

		private void AddOrUpdate(Dictionary<IPerson, int> dictionary, IPerson person)
		{
			if (!dictionary.ContainsKey(person))
				dictionary.Add(person, 0);

			dictionary[person]++;
		}
	}
}
