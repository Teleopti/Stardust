using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
    public static class JobStepResultFactory
    {
        public static IList<IJobStepResult> GetJobStepResultList(IBusinessUnit businessUnit, IList<IJobResult> jobResultCollection)
        {
            IList<IJobStepResult> jobStepResult = new List<IJobStepResult>();
            jobStepResult.Add(new JobStepResult("Name1", 1, 1, businessUnit, jobResultCollection));
            jobStepResult.Add(new JobStepResult("Name2", 2, 2, businessUnit, jobResultCollection));
            jobStepResult.Add(new JobStepResult("Name3", 3, 3, businessUnit, jobResultCollection));
            jobStepResult.Add(new JobStepResult("Name4", 0, new ArgumentException("job step result error"), businessUnit, jobResultCollection));

            return jobStepResult;
        }
    }
}