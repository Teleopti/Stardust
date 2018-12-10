using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillDayCreator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public PersonSkillDayCreator(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public PersonSkillDay Create(DateOnly date, IVirtualSchedulePeriod currentSchedulePeriod)
		{
			var personPeriod = currentSchedulePeriod.Person.Period(date);
			var skills = (from personSkill in _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(personPeriod)
										select personSkill.Skill).ToArray();

			var nonBlendSkills = (from personSkill in personPeriod.PersonNonBlendSkillCollection
							   where !((IDeleteTag)personSkill.Skill).IsDeleted
							   select personSkill.Skill).ToArray();

			var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(date.Date, currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));
			return new PersonSkillDay(period, personPeriod.Team, skills, nonBlendSkills);
		}
	}

	public class PersonSkillPeriodsDataHolderManager : IPersonSkillPeriodsDataHolderManager
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly ISkillPriorityProvider _skillPriorityProvider;

		public PersonSkillPeriodsDataHolderManager(Func<ISchedulingResultStateHolder> schedulingResultStateHolder, ISkillPriorityProvider skillPriorityProvider)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_skillPriorityProvider = skillPriorityProvider;
		}

		public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> GetPersonSkillPeriodsDataHolderDictionary(PersonSkillDay personSkillDay)
        {
            return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffDataPerActivity(personSkillDay.Period(), personSkillDay.Skills(), _skillPriorityProvider);
        }

		public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonNonBlendSkillSkillStaffPeriods(PersonSkillDay personSkillDay)
        {
			var nonBlendSkills = personSkillDay.NonBlendSkills();
			if (nonBlendSkills.IsEmpty())
                return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

            return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodDictionary(nonBlendSkills, personSkillDay.Period());
        }
    }
}
