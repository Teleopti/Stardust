using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStatePersonPeriod
	{
		bool PersonPeriodEquals(IPersonPeriod personPeriod);
	}

	public class TeamSteadyStatePersonPeriod : ITeamSteadyStatePersonPeriod
	{
		private readonly IPersonPeriod _personPeriod;

		public TeamSteadyStatePersonPeriod(IPersonPeriod personPeriod)
		{
			_personPeriod = personPeriod;
		}

		public bool PersonPeriodEquals(IPersonPeriod personPeriod)	
		{
			if(personPeriod == null) throw new ArgumentNullException("personPeriod");

			if (!SkillEquals(personPeriod)) return false;
			if (!_personPeriod.PersonContract.Contract.Equals(personPeriod.PersonContract.Contract)) return false;
			if (!_personPeriod.PersonContract.PartTimePercentage.Equals(personPeriod.PersonContract.PartTimePercentage)) return false;
			if (!_personPeriod.RuleSetBag.Equals(personPeriod.RuleSetBag)) return false;

			return true;
		}

		private bool SkillEquals(IPersonPeriod personPeriod)
		{
			if (!_personPeriod.PersonSkillCollection.Count.Equals(personPeriod.PersonSkillCollection.Count)) return false;

			return _personPeriod.PersonSkillCollection.All(personSkill => personPeriod.PersonSkillCollection.Contains(personSkill));
		}
	}
}
