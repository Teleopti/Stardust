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

		[UnitOfWork]
		public virtual IList<IPerson> FindPersonsByKeywords(string keywords)
		{
			var separateKeywords = keywords.Split(' ');
			var persons = _personRepository.LoadAll();

			var perfectMatch = persons.Where(p => keywords == $"{p.Name.FirstName} {p.Name.LastName}" ||
												  keywords == $"{p.Name.LastName} {p.Name.FirstName}").ToList();

			if (perfectMatch.Count == 1) return perfectMatch.ToList();

			var hitsPerPerson = new Dictionary<IPerson, int>();
			
			foreach (var keyword in separateKeywords)
			{
				persons.Where(p => p.Name.FirstName.Contains(keyword)).ForEach(p => AddOrUpdate(hitsPerPerson, p));
				persons.Where(p => p.Name.LastName.Contains(keyword)).ForEach(p => AddOrUpdate(hitsPerPerson, p));
			}

			return hitsPerPerson.OrderByDescending(kp => kp.Value).Select(refPersonCount => refPersonCount.Key).ToList();
		}

		private void AddOrUpdate(Dictionary<IPerson, int> dictionary, IPerson person)
		{
			if (!dictionary.ContainsKey(person))
				dictionary.Add(person, 0);

			dictionary[person]++;
		}

		[UnitOfWork]
		public virtual IList<IPerson> FindPersonsByKeywordsOld(string keywords)
		{
			var separateKeywords = keywords.Split(' ');
			var persons = _personRepository.LoadAll();
			var allMatches = separateKeywords.SelectMany(k =>
				persons.Where(p => p.Name.FirstName.Contains(k) || p.Name.LastName.Contains(k))).Distinct();

			return allMatches.ToList();
		}
	}
}
