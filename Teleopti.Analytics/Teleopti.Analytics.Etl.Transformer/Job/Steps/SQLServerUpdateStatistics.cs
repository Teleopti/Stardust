using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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