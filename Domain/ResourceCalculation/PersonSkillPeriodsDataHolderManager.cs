﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PersonSkillPeriodsDataHolderManager : IPersonSkillPeriodsDataHolderManager
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

        public PersonSkillPeriodsDataHolderManager(Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }

		public IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> GetPersonSkillPeriodsDataHolderDictionary(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod)
        {
			InParameter.NotNull("currentSchedulePeriod", currentSchedulePeriod);
		    var personPeriod = currentSchedulePeriod.Person.Period(scheduleDateOnly);
            var skills = (from personSkill in personPeriod.PersonSkillCollection
                                     where !((IDeleteTag) personSkill.Skill).IsDeleted & personSkill.Active
						  select personSkill.Skill).ToArray();

            var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(scheduleDateOnly.Date,
																  currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
            var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));
            return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffDataPerActivity(period, skills);
        }

        public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonMaxSeatSkillSkillStaffPeriods(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod)
        {
            var site = currentSchedulePeriod.Person.Period(scheduleDateOnly).Team.Site;
			if(site.MaxSeatSkill == null)
				return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

            var personPeriod = currentSchedulePeriod.Person.Period(scheduleDateOnly);
            IEnumerable<ISkill> maxSeatSkills = (from personSkill in personPeriod.PersonMaxSeatSkillCollection
								 where !((IDeleteTag)personSkill.Skill).IsDeleted
												 select personSkill.Skill).ToArray();
			
		if(maxSeatSkills.IsEmpty())
			return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

			var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(scheduleDateOnly.Date,
																  currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
			var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));


			return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodDictionary(maxSeatSkills, period);
		}

        public IDictionary<ISkill, ISkillStaffPeriodDictionary> GetPersonNonBlendSkillSkillStaffPeriods(DateOnly scheduleDateOnly, IVirtualSchedulePeriod currentSchedulePeriod)
        {
            var personPeriod = currentSchedulePeriod.Person.Period(scheduleDateOnly);
            IEnumerable<ISkill> nonBlendSkills = (from personSkill in personPeriod.PersonNonBlendSkillCollection
                                                 where !((IDeleteTag)personSkill.Skill).IsDeleted
                                                 select personSkill.Skill).ToArray();

            if (nonBlendSkills.IsEmpty())
                return new Dictionary<ISkill, ISkillStaffPeriodDictionary>();

            var scheduleDayUtc = TimeZoneHelper.ConvertToUtc(scheduleDateOnly.Date,
                                                                  currentSchedulePeriod.Person.PermissionInformation.DefaultTimeZone());
            var period = new DateTimePeriod(scheduleDayUtc, scheduleDayUtc.AddDays(2));


            return _schedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodDictionary(nonBlendSkills, period);
        }
    }
}
