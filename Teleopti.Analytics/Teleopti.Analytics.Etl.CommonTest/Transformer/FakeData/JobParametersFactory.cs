using System.Globalization;
using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
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
				new FakeContainerHolder(),
				false
			)
			{
				Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null)
			};


			return jobParameters;
		}

		public static IJobParameters SimpleParameters(IJobHelper jobHelper, int intervalLength)
		{
			var jobParameters = new JobParameters(
				JobMultipleDateFactory.CreateJobMultipleDate(), 1, "W. Europe Standard Time", intervalLength,
				"Data Source=SSAS_Server;Initial Catalog=SSAS_DB",
				"False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(),
				false
			)
			{
				Helper = jobHelper
			};


			return jobParameters;
		}

		public class FakeContainerHolder : IContainerHolder
		{
			public FakeContainerHolder()
			{
				ToggleManager = new FakeToggleManager();
				TenantLogonInfoLoader = new FakeTenantLogonInfoLoader();
			}
			public IToggleManager ToggleManager { get; private set; }
			public ITenantLogonInfoLoader TenantLogonInfoLoader { get; private set; }
			public IComponentContext IocContainer { get; set; }


			public void EnableToggle(Toggles toggle)
			{
				((FakeToggleManager)ToggleManager).Enable(toggle);
			}

			public void DisableToggle(Toggles toggle)
			{
				((FakeToggleManager)ToggleManager).Enable(toggle);
			}
		}
	}
}
