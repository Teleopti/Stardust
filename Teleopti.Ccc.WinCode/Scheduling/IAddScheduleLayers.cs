using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IAddScheduleLayers
    {
 
        IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IEnumerable<IAbsence> bindingList, ISetupDateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddActivityViewModel CreateAddActivityViewModel(IEnumerable<IActivity> activities,IList<IShiftCategory> shiftCategories,DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IEnumerable<IActivity> activities, DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddOvertimeViewModel CreateAddOvertimeViewModel(IScheduleDay selectedSchedule,IEnumerable<IActivity> activities,
                                                         IList<IMultiplicatorDefinitionSet> definitionSets,
                                                         IActivity defaultActivity, DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IEnumerable<IDayOffTemplate> dayOffTemplates, TimeZoneInfo timeZoneInfo, DateTimePeriod period);
    }
}