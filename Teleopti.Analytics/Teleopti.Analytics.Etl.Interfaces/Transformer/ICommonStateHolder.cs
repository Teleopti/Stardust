using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface ICommonStateHolder : IGroupPageDataProvider
    {
        IList<IScenario> ScenarioCollection { get; }
        IScenario DefaultScenario { get; }
        IList<TimeZoneInfo> TimeZoneCollection { get; }
        DateTimePeriod? PeriodToLoadBridgeTimeZone { get; }
        IScheduleDictionary GetSchedules(DateTimePeriod period, IScenario scenario);
        ICollection<ISkillDay> GetSkillDaysCollection(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
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

        /// <summary>
        /// Loads the schedule parts per person and date.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Henryg
        /// Created date: 2009-11-24
        /// </remarks>
        IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScenario scenario);
    }
}