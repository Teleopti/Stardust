using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
    public interface IJob : INotifyPropertyChanged
    {
        IList<IJobStep> StepList { get; }
        String Name { get; set; }
        IJobResult Run(IBusinessUnit businessUnit, IList<IJobStep> jobStepsNotToRun, IList<IJobResult> jobResultCollection, bool isFirstBusinessUnitRun, bool isLastBusinessUnitRun);
        bool NeedsParameterDatePeriod { get; }
        bool NeedsParameterDataSource { get; }
        IJobResult Result { get; }
        ReadOnlyCollection<JobCategoryType> JobCategoryCollection { get; }
        bool Enabled { get; set; }
        event EventHandler<AlarmEventArgs> JobExecutionReady;
        void NotifyJobExecutionReady();
		  IJobParameters JobParameters { get; }
    }
}