using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IJobResult : INotifyPropertyChanged
    {
        double Duration { get; }
        void Update();
        int RowsAffected { get; }
        IList<IJobStepResult> JobStepResultCollection { get; }
        String Name { set; get; }
        string Status { set; get; }
        bool Success { set; get; }
        string BusinessUnitStatus { get; }
        IBusinessUnit CurrentBusinessUnit { get; }
        bool HasError { get; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
    }
}
