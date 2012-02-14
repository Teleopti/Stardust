using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
	public class RequestJobCollection : List<IJobStep>
	{
	
		public RequestJobCollection(IJobParameters jobParameters)
        {
            Add(new StageRequestJobStep(jobParameters));
			Add(new FactRequestJobStep(jobParameters));
        }
	}
}