using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	class RaptorAgentLogOnSynchronizationStep : JobStepBase
	{
		public RaptorAgentLogOnSynchronizationStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "TeleoptiCCC7.ExternalLogOn";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			ReadOnlyCollection<IExternalLogOn> externalLogOns =
				 _jobParameters.Helper.Repository.LoadAgentLogins();
			return _jobParameters.Helper.Repository.SynchronizeAgentLogOns(externalLogOns);
		}
	}
}
