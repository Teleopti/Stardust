using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
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