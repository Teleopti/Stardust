using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	public class FakeSkillStaffPeriodHolder :ISkillStaffPeriodHolder, IShovelResourceData
	{
		public void SetDictionary(ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodDictionary)
		{
			SkillSkillStaffPeriodDictionary = skillSkillStaffPeriodDictionary;
		}
			
		
		public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> SkillStaffDataPerActivity(DateTimePeriod onPeriod, IList<ISkill> onSkills,
			ISkillPriorityProvider skillPriorityProvider)
		{
			throw new NotImplementedException();
		}

		public ISkillSkillStaffPeriodExtendedDictionary SkillSkillStaffPeriodDictionary { get; private set; }
		public IList<ISkillStaffPeriod> SkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			var skillStaffPeriods = new List<ISkillStaffPeriod>();
			skills.ForEach(skill =>
			{
				ISkillStaffPeriodDictionary content;
				if (SkillSkillStaffPeriodDictionary.TryGetValue(skill, out content))
				{
					foreach (var dictionary in content)
					{
						if (dictionary.Key.EndDateTime <= utcPeriod.StartDateTime) continue;
						if (dictionary.Key.StartDateTime >= utcPeriod.EndDateTime) continue;

						skillStaffPeriods.Add(dictionary.Value);
					}
				}
			});
			return skillStaffPeriods;
		}

		public IList<ISkillStaffPeriod> SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod)
		{
			throw new NotImplementedException();
		}

		public ISkillStaffPeriodDictionary SkillStaffPeriodList(IAggregateSkill skill, DateTimePeriod utcPeriod, bool forDay)
		{
			throw new NotImplementedException();
		}

		public IList<ISkillStaffPeriod> IntersectingSkillStaffPeriodList(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			throw new NotImplementedException();
		}

		public IDictionary<ISkill, ISkillStaffPeriodDictionary> SkillStaffPeriodDictionary(IEnumerable<ISkill> skills, DateTimePeriod utcPeriod)
		{
			throw new NotImplementedException();
		}

		public bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval)
		{
			throw new NotImplementedException();
		}
	}
}