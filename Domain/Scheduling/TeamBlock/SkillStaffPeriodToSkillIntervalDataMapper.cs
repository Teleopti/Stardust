using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillStaffPeriodToSkillIntervalDataMapper
    {
        IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriod, DateOnly baseDate, TimeZoneInfo timeZoneInfo);
    }

    public class SkillStaffPeriodToSkillIntervalDataMapper : ISkillStaffPeriodToSkillIntervalDataMapper
    {
	    public IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriodList,DateOnly baseDate, TimeZoneInfo timeZoneInfo)
	    {
		    var skillIntervalList = new List<ISkillIntervalData>();
		    if (skillStaffPeriodList != null)
			    foreach (var skillStaffPeriod in skillStaffPeriodList)
			    {
				    int minStaff = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons;
				    int maxStaff = skillStaffPeriod.Payload.SkillPersonData.MaximumPersons;
				    var currentHead = skillStaffPeriod.Payload.CalculatedLoggedOn;
				    var utcPeriod = skillStaffPeriod.Period;
				    var localStartTime = DateTime.SpecifyKind(utcPeriod.StartDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
					var localEndTime = localStartTime.Add(utcPeriod.ElapsedTime());
				    var localPeriod = new DateTimePeriod(localStartTime, localEndTime);
				    skillIntervalList.Add(new SkillIntervalData(localPeriod, skillStaffPeriod.FStaff,
					    skillStaffPeriod.FStaff - skillStaffPeriod.CalculatedResource, currentHead, minStaff, maxStaff));
			    }
		    return skillIntervalList;
	    }
    }
}