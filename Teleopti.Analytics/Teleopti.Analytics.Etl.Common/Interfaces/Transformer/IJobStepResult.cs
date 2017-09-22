using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
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