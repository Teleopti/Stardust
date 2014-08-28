using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
	public static class JobParametersFactory
	{
		public static IJobParameters SimpleParameters(bool isPMInstalled)
		{
			var jobParameters = new JobParameters(JobMultipleDateFactory.CreateJobMultipleDate(), 1, "W. Europe Standard Time", 5,
									 "Data Source=SSAS_Server;Initial Catalog=SSAS_DB",
									 isPMInstalled.ToString(CultureInfo.InvariantCulture),
									 CultureInfo.CurrentCulture);

			jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);

			return jobParameters;
		}
	}
}
