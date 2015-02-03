using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IJobStepResult : INotifyPropertyChanged, ICloneable
    {
        double? Duration { get; set; }
        int? RowsAffected { get; set; }
        String Name { get; }
        string Status { get; set; }
        Exception JobStepException { get; }
        string BusinessUnitStatus { get; }
        IBusinessUnit CurrentBusinessUnit { get; }
        bool HasError { get; }
        void ClearResult();
    }
}