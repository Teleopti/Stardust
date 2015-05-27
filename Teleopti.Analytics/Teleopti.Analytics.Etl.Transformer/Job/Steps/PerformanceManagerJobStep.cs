using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class PerformanceManagerJobStep : JobStepBase
    {
        public PerformanceManagerJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "Process Cube";
            IsBusinessUnitIndependent = false; //It is actually BU independent, but it should be run for the last BU, not the first
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
        	int timeZoneId = _jobParameters.Helper.Repository.DefaultTimeZone.MartId;
			//When last bu then process PM cube
            if (isLastBusinessUnit)
                new CubeRepository(_jobParameters.OlapServer, _jobParameters.OlapDatabase, timeZoneId).Process();

            return 0;
        }
    }
}