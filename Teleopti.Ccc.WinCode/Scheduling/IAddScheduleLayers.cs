using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IAddScheduleLayers
    {
 
        IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IList<IAbsence> bindingList, ISetupDateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddActivityViewModel CreateAddActivityViewModel(IList<IActivity> activities,IList<IShiftCategory> shiftCategories,DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IList<IActivity> activities, DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddOvertimeViewModel CreateAddOvertimeViewModel(IScheduleDay selectedSchedule,IList<IActivity> activities,
                                                         IList<IMultiplicatorDefinitionSet> definitionSets,
                                                         IActivity defaultActivity, DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IList<IDayOffTemplate> dayOffTemplates, TimeZoneInfo timeZoneInfo, DateTimePeriod period);
    }
}