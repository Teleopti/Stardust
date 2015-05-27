using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class BridgeTimeZoneJobStep : JobStepBase
    {
        public BridgeTimeZoneJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "bridge_time_zone";
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Load data from stage to datamart
            return _jobParameters.Helper.Repository.FillTimeZoneBridgeDataMart();
        }
    }
}