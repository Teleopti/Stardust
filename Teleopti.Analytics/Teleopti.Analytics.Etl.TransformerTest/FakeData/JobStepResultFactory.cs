using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
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

        //public static IList<IJobStepResult> GetJobStepResultList()
        //{
        //    IList<IJobStepResult> jobStepResult = new List<IJobStepResult>();
        //    jobStepResult.Add(new Result("Name1", 1, 1));
        //    jobStepResult.Add(new Result("Name2", 2, 2));
        //    jobStepResult.Add(new Result("Name3", 3, 3));
        //    return jobStepResult;
        //}
    }
}