using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class InitialJobCollection:List<IJobStep>
    {
        public InitialJobCollection(IJobParameters jobParameters)
        {
            Add(new DimIntervalJobStep(jobParameters));     // BU independent
            Add(new StageDateJobStep(jobParameters));       // BU independent
            Add(new DimDateJobStep(jobParameters));         // BU independent
            Add(new StageTimeZoneJobStep(jobParameters));
            Add(new DimTimeZoneJobStep(jobParameters));
            Add(new StageTimeZoneBridgeJobStep(jobParameters));
            Add(new BridgeTimeZoneJobStep(jobParameters));
        }    
   }

}