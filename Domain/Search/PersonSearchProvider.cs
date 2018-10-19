using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
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

		[UnitOfWork]
		public virtual IList<IPerson> FindPersonsByKeywords(string keywords)
		{
			var separateKeywords = keywords.Split(' ');
			var persons = _personRepository.LoadAll();
			return separateKeywords.SelectMany(k => 
				persons.Where(p => p.Name.FirstName.Contains(k) || p.Name.LastName.Contains(k))).Distinct().ToList();
		}
	}
}
