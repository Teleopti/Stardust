using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	class RaptorQueueSynchronizationStep : JobStepBase
	{
		public RaptorQueueSynchronizationStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "TeleoptiCCC7.QueueSource";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			ReadOnlyCollection<IQueueSource> queueSources =
				 _jobParameters.Helper.Repository.LoadQueues();
			return _jobParameters.Helper.Repository.SynchronizeQueues(queueSources);
		}
	}
}
