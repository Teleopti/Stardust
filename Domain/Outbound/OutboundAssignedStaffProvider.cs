﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class OutboundAssignedStaffProvider
	{
		private readonly IPersonRepository _personRepository;

		public OutboundAssignedStaffProvider(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public PeopleSelection Load(IList<Campaign> campaigns, DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindPeopleInOrganizationLight(period).ToList();
			var affectedSkills = campaigns.Select(campaign => campaign.Skill).ToList();

			var selectedPeople = new HashSet<IPerson>();
			foreach (var person in allPeople)
			{
				var intersectingPeriods = person.PersonPeriods(period);
				foreach (var personPeriod in intersectingPeriods)
				{
					foreach (var personSkill in personPeriod.PersonSkillCollection)
					{
						var skill = personSkill.Skill;
						if (affectedSkills.Contains(skill) && personSkill.Active)
							selectedPeople.Add(person);
					}
				}
			}

			return new PeopleSelection(allPeople, selectedPeople.ToList());
		}
	}
}