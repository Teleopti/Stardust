using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public interface IDayLayerViewModel : INotifyPropertyChanged
    {
		void RefreshElapsedTime(DateTime timestamp);
        void RefreshProjection(IPerson person);
        void UnregisterMessageBrokerEvent();
        void OnScheduleModified(object sender, ModifyEventArgs e);
	    void InitializeRows();
        ICollection<DayLayerModel> Models { get; }
        void CreateModels(IEnumerable<IPerson> people, IDateOnlyPeriodAsDateTimePeriod period);
    }
}
