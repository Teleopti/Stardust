using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimPersonTrimJobStep : JobStepBase
	{
		public DimPersonTrimJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "dim_person trim data";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return _jobParameters.Helper.Repository.DimPersonTrimData(RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}