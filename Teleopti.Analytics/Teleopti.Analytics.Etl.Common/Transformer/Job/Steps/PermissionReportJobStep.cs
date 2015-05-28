using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class PermissionReportJobStep : JobStepBase
	{
		private readonly bool _checkIfNeeded;

		public PermissionReportJobStep(IJobParameters jobParameters, bool checkIfNeeded = false)
			: base(jobParameters)
		{
			_checkIfNeeded = checkIfNeeded;
			Name = "permission_report";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			if (_checkIfNeeded)
			{
				if (!_jobParameters.StateHolder.PermissionsMustRun()) return 0;
			}

			var rows = _jobParameters.Helper.Repository.FillPermissionDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
			_jobParameters.StateHolder.UpdateThisTime("Permissions", RaptorTransformerHelper.CurrentBusinessUnit);
			return rows;
		}
	}
}