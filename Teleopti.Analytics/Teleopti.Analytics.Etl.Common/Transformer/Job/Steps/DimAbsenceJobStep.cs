using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimAbsenceJobStep : JobStepBase
	{
		public DimAbsenceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "dim_absence";
		}


		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{

			return _jobParameters.Helper.Repository.FillAbsenceDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}