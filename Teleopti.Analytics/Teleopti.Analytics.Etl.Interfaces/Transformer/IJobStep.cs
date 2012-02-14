using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IJobStep
    {
        String Name { get; }
        IJobStepResult Run(IList<IJobStep> jobStepsNotToRun, IBusinessUnit currentBusinessUnit, IList<IJobResult> jobResultCollection, bool isLastBusinessUnitRun);
        IJobStepResult Result { get; }
        IJobParameters JobParameters { get; }
        JobCategoryType JobCategory { get; }
        void SetResult(IJobStepResult jobStepResult);
        bool IsBusinessUnitIndependent { get; }
    }
}