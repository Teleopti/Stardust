using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.Models;


namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public class PersonDataProvider : IPersonDataProvider
	{
		private readonly IPersonRepository _personRepo;

		public PersonDataProvider(IPersonRepository personRepo)
		{
			_personRepo = personRepo;
		}

		public IEnumerable<PersonDataModel> RetrievePeople(DateTime date, IEnumerable<Guid> personIdList)
		{
			var people = _personRepo.FindPeople(personIdList);
			var result = people.Select(p =>
			{
				var currentPeriod =
					p.PersonPeriods(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date))).ToList().Single();
                return new PersonDataModel
				{
					PersonId = p.Id.GetValueOrDefault(),
					FirstName = p.Name.FirstName,
					LastName = p.Name.LastName,
					Team = currentPeriod.Team.SiteAndTeam,
					SkillIdList = currentPeriod.PersonSkillCollection.Select(s => s.Skill.Id.GetValueOrDefault()).ToList(),
					ShiftBagId = currentPeriod.RuleSetBag != null ? currentPeriod.RuleSetBag.Id : null
				};
			}).ToList();

			return result;
		}
	}
}