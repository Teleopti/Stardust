﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public interface IDayLayerViewModel : INotifyPropertyChanged
    {
        void Refresh(DateTime timestamp);
        void RefreshProjection(IPerson person);
        void UnregisterMessageBrokerEvent();

        void OnScheduleModified(object sender, ModifyEventArgs e);

        ICollection<DayLayerModel> Models { get; }
        void CreateModels(IEnumerable<IPerson> people, IDateOnlyPeriodAsDateTimePeriod period);
    }
}
