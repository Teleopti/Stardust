using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillDayCreator : IPersonSkillDayCreator
	{
		public PersonSkillDay Create(DateOnly date, IVirtualSchedulePeriod currentSchedulePeriod)
		{
			var personPeriod = currentSchedulePeriod.Person.Period(date);
			var skills = (from personSkill in personPeriod.PersonSkillCollection
					   where !((IDeleteTag)personSkill.Skill).IsDeleted & personSkill.Active
					   select personSkill.Skill).ToArray();

			var maxSeatSkills = (from personSkill in personPeriod.PersonMaxSeatSkillCollection
							  where !((IDeleteTag)personSkill.Skill).IsDeleted
							  select personSkill.Skill).ToArray();

			var nonBlendSkills = (from personSkill in personPeriod.PersonNonBlendSkillCollection
							   where !((IDeleteTag)personSkill.Skill).IsDeleted
							   select personSkill.Skill).ToArray();

			var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(date.Date, currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));
			return new PersonSkillDay(period, personPeriod.Team, skills, maxSeatSkills, nonBlendSkills);
		}
	}

	public class PersonSkillPeriodsDataHolderManager : IPersonSkillPeriodsDataHolderManager
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

        public PersonSkillPeriodsDataHolderManager(Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

		public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> GetPersonSkillPeriodsDataHolderDictionary(PersonSkillDay personSkillDay)
        {
            return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffDataPerActivity(personSkillDay.Period(), personSkillDay.Skills());
        }

		public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonMaxSeatSkillSkillStaffPeriods(
			PersonSkillDay personSkillDay)
		{
			var site = personSkillDay.Team().Site;
			if (site.MaxSeatSkill == null)
				return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

			var maxSeatSkills = personSkillDay.MaxSeatSkills();
			if (maxSeatSkills.IsEmpty())
				return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

			return _schedulingResultStateHolder()
				.SkillStaffPeriodHolder.SkillStaffPeriodDictionary(maxSeatSkills, personSkillDay.Period());
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
