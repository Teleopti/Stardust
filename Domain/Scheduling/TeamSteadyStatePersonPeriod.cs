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

			if (!SkillEquals(personPeriod)) 
				return false;
			if (!_personPeriod.PersonContract.Contract.Equals(personPeriod.PersonContract.Contract)) 
				return false;
			if (!_personPeriod.PersonContract.PartTimePercentage.Equals(personPeriod.PersonContract.PartTimePercentage)) 
				return false;
			if (_personPeriod.RuleSetBag == null || !_personPeriod.RuleSetBag.Equals(personPeriod.RuleSetBag)) 
				return false;

			return true;
		}

		private bool SkillEquals(IPersonPeriod personPeriod)
		{
			if (!_personPeriod.PersonSkillCollection.Count().Equals(personPeriod.PersonSkillCollection.Count())) 
				return false;

			foreach (var firstPersonSkill in _personPeriod.PersonSkillCollection)
			{
				var exists = false;

				foreach (var secondPersonSkill in personPeriod.PersonSkillCollection)
				{
					var skill1 = firstPersonSkill.Skill;
					var skill2 = secondPersonSkill.Skill;

					if (skill1.Equals(skill2))
					{
						exists = true;
						break;
					}
				}

				if (!exists)
					return false;
			}

			return true;
		}
	}
}
