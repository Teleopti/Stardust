using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class InsightsDataRefreshJobCollection : List<IJobStep>
	{
		public InsightsDataRefreshJobCollection(IJobParameters jobParameters)
		{
			Add(new TriggerInsightsDataRefreshJobStep(jobParameters, new ServiceBusTopicClientFactory()));
		}
	}
}
