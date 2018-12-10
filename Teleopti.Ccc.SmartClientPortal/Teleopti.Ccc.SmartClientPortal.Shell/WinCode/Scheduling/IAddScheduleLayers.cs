using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface IAddScheduleLayers
    {
 
        IAddLayerViewModel<IAbsence> CreateAddAbsenceViewModel(IEnumerable<IAbsence> bindingList, ISetupDateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddActivityViewModel CreateAddActivityViewModel(IEnumerable<IActivity> activities, IList<IShiftCategory> shiftCategories, DateTimePeriod period, TimeZoneInfo timeZoneInfo, IActivity defaultActivity);

        IAddLayerViewModel<IActivity> CreateAddPersonalActivityViewModel(IEnumerable<IActivity> activities, DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddOvertimeViewModel CreateAddOvertimeViewModel(IEnumerable<IActivity> activities,
                                                         IList<IMultiplicatorDefinitionSet> definitionSets,
                                                         IActivity defaultActivity, DateTimePeriod period, TimeZoneInfo timeZoneInfo);

        IAddLayerViewModel<IDayOffTemplate> CreateAddDayOffViewModel(IEnumerable<IDayOffTemplate> dayOffTemplates, TimeZoneInfo timeZoneInfo, DateTimePeriod period);
    }
}