using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.ReadModel;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface ICommonStateHolder : IGroupPageDataProvider
    {
        IList<IScenario> ScenarioCollection { get; }
        IScenario DefaultScenario { get; }
        IList<TimeZoneInfo> TimeZoneCollection { get; }
        IList<TimeZonePeriod> PeriodToLoadBridgeTimeZone { get; }
        IScheduleDictionary GetSchedules(DateTimePeriod period, IScenario scenario);
        ICollection<ISkillDay> GetSkillDaysCollection(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
		ICollection<ISkillDay> GetSkillDaysCollection(IScenario scenario, DateTime lastCheck);
        IDictionary<ISkill, IList<ISkillDay>> GetSkillDaysDictionary(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
        IList<IPerson> UserCollection { get; }
        IList<IActivity> ActivityCollection { get; }
        IList<IAbsence> AbsenceCollection { get; }
        IList<IDayOffTemplate> DayOffTemplateCollection { get; }
        IList<IShiftCategory> ShiftCategoryCollection { get; }
        IList<IApplicationFunction> ApplicationFunctionCollection { get; }
        IList<IApplicationRole> ApplicationRoleCollection { get; }
        IList<IAvailableData> AvailableDataCollection { get; }
        IList<IScenario> ScenarioCollectionDeletedExcluded { get; }
        IList<IScheduleDay> GetSchedulePartPerPersonAndDate(IScheduleDictionary scheduleDictionary);
    	IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetCollection { get; }
        IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScenario scenario);

	    IDictionary<DateTimePeriod, IScheduleDictionary> GetSchedules(IList<IScheduleChangedReadModel> changed,
																	IScenario scenario);

	    IDictionary<DateOnly, IScheduleDictionary> GetSchedules(HashSet<IStudentAvailabilityDay> days,
		    IScenario scenario);
	    IDictionary<DateTimePeriod, IScheduleDictionary> GetScheduleCache();

	    IList<IScheduleDay> GetSchedulePartPerPersonAndDate(
		    IDictionary<DateTimePeriod, IScheduleDictionary> dictionary);

	    IList<IPerson> PersonsWithIds(List<Guid> ids);
		IScheduleDay GetSchedulePartOnPersonAndDate(IPerson person, DateOnly restrictionDate, IScenario scenario);
	    void SetThisTime(ILastChangedReadModel lastTime, string step);
	    void UpdateThisTime(string step, IBusinessUnit businessUnit);
	    bool PermissionsMustRun();
		 void SetLoadBridgeTimeZonePeriod(DateTimePeriod period, string timeZoneCode);
    }
}