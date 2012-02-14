using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

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

            //DateTimePeriod period = new DateTimePeriod(JobParameters.StartDateUtc, JobParameters.EndDateUtc.AddDays(1));
            DateTimePeriod? period = _jobParameters.StateHolder.PeriodToLoadBridgeTimeZone;
            if (!period.HasValue)
            {
                // No valid period - skip job step
                return 0;
            }

            //Load data from stage to datamart
            return _jobParameters.Helper.Repository.FillTimeZoneBridgeDataMart(period.Value);

        }
    }
}