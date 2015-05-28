using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class SqlServerUpdateStatistics : JobStepBase
	{
		public SqlServerUpdateStatistics(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "sqlserver_updatestat";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return _jobParameters.Helper.Repository.SqlServerUpdateStatistics();
		}
	}
}