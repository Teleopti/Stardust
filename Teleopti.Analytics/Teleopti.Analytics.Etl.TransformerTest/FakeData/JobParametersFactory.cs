using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
	public static class JobParametersFactory
	{
		public static IJobParameters SimpleParameters(bool isPmInstalled)
		{
			var jobParameters = new JobParameters(
				JobMultipleDateFactory.CreateJobMultipleDate(), 1, "W. Europe Standard Time", 5,
				"Data Source=SSAS_Server;Initial Catalog=SSAS_DB",
				isPmInstalled.ToString(CultureInfo.InvariantCulture),
				CultureInfo.CurrentCulture, 
				new FakeToggleManager(),
				false
			);

			jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);

			return jobParameters;
		}
	}
}
