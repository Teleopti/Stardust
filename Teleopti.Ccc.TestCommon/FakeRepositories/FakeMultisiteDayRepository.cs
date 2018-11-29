using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMultisiteDayRepository : IMultisiteDayRepository
	{
		private readonly List<IMultisiteDay> storage = new List<IMultisiteDay>();

		public ICollection<IMultisiteDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario)
		{
			return
				storage.Where(m => m.Skill.Equals(skill) && period.Contains(m.MultisiteDayDate) && m.Scenario.Equals(scenario))
					.ToArray();
		}

		public ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill,
			IScenario scenario, bool addToRepository = true)
		{
			throw new NotImplementedException();
		}

		public void Has(params IMultisiteDay[] multisiteDays)
		{
			storage.AddRange(multisiteDays);
		}

		public void Delete(DateOnlyPeriod dateTimePeriod, IMultisiteSkill multisiteSkill, IScenario scenario)
		{
			throw new NotImplementedException();
		}
	}
}