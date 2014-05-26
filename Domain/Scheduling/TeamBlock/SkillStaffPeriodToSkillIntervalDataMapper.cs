using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillStaffPeriodToSkillIntervalDataMapper
    {
        IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriod, DateOnly baseDate, TimeZoneInfo timeZoneInfo);
    }

    public class SkillStaffPeriodToSkillIntervalDataMapper : ISkillStaffPeriodToSkillIntervalDataMapper
    {
	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
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
				    var localEndTime = DateTime.SpecifyKind(utcPeriod.EndDateTimeLocal(timeZoneInfo), DateTimeKind.Utc);
				    var localPeriod = new DateTimePeriod(localStartTime, localEndTime);
				    skillIntervalList.Add(new SkillIntervalData(localPeriod, skillStaffPeriod.FStaff,
					    skillStaffPeriod.FStaff - skillStaffPeriod.CalculatedResource, currentHead, minStaff, maxStaff));
			    }
		    return skillIntervalList;
	    }
    }
}