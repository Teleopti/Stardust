using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimAcdLogOnJobStep : JobStepBase
	{
		public DimAcdLogOnJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "dim_acd_login";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Load data from stage to datamart
			return _jobParameters.Helper.Repository.FillAcdLogOnDataMart(_jobParameters.DataSource);
		}
	}
}