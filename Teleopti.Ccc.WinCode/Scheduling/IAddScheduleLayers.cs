using System.Collections.Generic;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IAddScheduleLayers
    {
 
        IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IList<IAbsence> bindingList, ISetupDateTimePeriod period);

        IAddActivityViewModel CreateAddActivityViewModel(IList<IActivity> activities,IList<IShiftCategory> shiftCategories,DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo);

        IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IList<IActivity> activities, DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo);

        IAddOvertimeViewModel CreateAddOvertimeViewModel(IScheduleDay selectedSchedule,IList<IActivity> activities,
                                                         IList<IMultiplicatorDefinitionSet> definitionSets,
                                                         IActivity defaultActivity, DateTimePeriod period);

        IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IList<IDayOffTemplate> dayOffTemplates, ICccTimeZoneInfo timeZoneInfo, DateTimePeriod period);
    }
}