using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimOvertimeJobStep : JobStepBase
	{
		public DimOvertimeJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "dim_overtime";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Load data from stage to datamart
			return _jobParameters.Helper.Repository.FillOvertimeDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}