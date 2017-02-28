using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillDayCreator : IPersonSkillDayCreator
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public PersonSkillDayCreator(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		[RemoveMeWithToggle("Remove maxseatskills from PersonSkillDay below", Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public PersonSkillDay Create(DateOnly date, IVirtualSchedulePeriod currentSchedulePeriod)
		{
			var personPeriod = currentSchedulePeriod.Person.Period(date);
			var skills = (from personSkill in _personalSkillsProvider.PersonSkillsBasedOnPrimarySkill(personPeriod)
										select personSkill.Skill).ToArray();

			var maxSeatSkills = personPeriod.MaxSeatSkill;

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

		[RemoveMeWithToggle("Remove this method from interface all together",Toggles.ResourcePlanner_MaxSeatsNew_40939)]
		public virtual IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonMaxSeatSkillSkillStaffPeriods(PersonSkillDay personSkillDay)
		{
			return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
		}

		public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonNonBlendSkillSkillStaffPeriods(PersonSkillDay personSkillDay)
        {
			var nonBlendSkills = personSkillDay.NonBlendSkills();
			if (nonBlendSkills.IsEmpty())
                return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

            return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodDictionary(nonBlendSkills, personSkillDay.Period());
        }
    }

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class PersonSkillPeriodsDataHolderManagerOld : PersonSkillPeriodsDataHolderManager
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public PersonSkillPeriodsDataHolderManagerOld(Func<ISchedulingResultStateHolder> schedulingResultStateHolder, ISkillPriorityProvider skillPriorityProvider) : 
			base(schedulingResultStateHolder, skillPriorityProvider)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public override IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonMaxSeatSkillSkillStaffPeriods(PersonSkillDay personSkillDay)
		{
			var site = personSkillDay.Team().Site;
			if (site.MaxSeatSkill == null)
				return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

			var maxSeatSkill = personSkillDay.MaxSeatSkill();
			if (maxSeatSkill == null)
				return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

			return _schedulingResultStateHolder()
				.SkillStaffPeriodHolder.SkillStaffPeriodDictionary(new[] { maxSeatSkill }, personSkillDay.Period());
		}
	}
}
