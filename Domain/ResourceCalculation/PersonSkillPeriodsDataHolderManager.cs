using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PersonSkillPeriodsDataHolderManager : IPersonSkillPeriodsDataHolderManager
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        public PersonSkillPeriodsDataHolderManager(ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

		public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> GetPersonSkillPeriodsDataHolderDictionary(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod)
        {
			InParameter.NotNull("currentSchedulePeriod", currentSchedulePeriod);
		    var personPeriod = currentSchedulePeriod.Person.Period(scheduleDateOnly);
            var skills = (from personSkill in personPeriod.PersonSkillCollection
                                     where !((IDeleteTag) personSkill.Skill).IsDeleted & personSkill.Active
                                     select personSkill.Skill).ToList();

            var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(scheduleDateOnly.Date,
																  currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
            var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));
            return _schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffDataPerActivity(period, skills);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonMaxSeatSkillSkillStaffPeriods(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod)
        {
            var site = currentSchedulePeriod.Person.Period(scheduleDateOnly).Team.Site;
			if(site.MaxSeatSkill == null)
				return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

            var personPeriod = currentSchedulePeriod.Person.Period(scheduleDateOnly);
            IEnumerable<ISkill> maxSeatSkills = (from personSkill in personPeriod.PersonMaxSeatSkillCollection
								 where !((IDeleteTag)personSkill.Skill).IsDeleted
								 select personSkill.Skill).ToList();
			
		if(maxSeatSkills.IsEmpty())
			return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

			var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(scheduleDateOnly.Date,
																  currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));


			return _schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodDictionary(maxSeatSkills, period);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonNonBlendSkillSkillStaffPeriods(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod)
        {
            var personPeriod = currentSchedulePeriod.Person.Period(scheduleDateOnly);
            IEnumerable<ISkill> nonBlendSkills = (from personSkill in personPeriod.PersonNonBlendSkillCollection
                                                 where !((IDeleteTag)personSkill.Skill).IsDeleted
                                                 select personSkill.Skill).ToList();

            if (nonBlendSkills.IsEmpty())
                return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

            var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(scheduleDateOnly.Date,
                                                                  currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
            var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));


            return _schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodDictionary(nonBlendSkills, period);
        }
    }
}
