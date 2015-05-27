using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class LicenseCheckJobStep : JobStepBase
    {
        public LicenseCheckJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            // another more discreate name??????
            Name = "PrimaryCheck";
            JobCategory = JobCategoryType.Initial;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {

            return JobParameters.Helper.Repository.LicenseStatusUpdater.RunCheck();

        }
    }
}