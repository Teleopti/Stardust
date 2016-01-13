using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;

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
			);

			jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);

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

	public class FakeTenantLogonInfoLoader : ITenantLogonInfoLoader
	{
		public IEnumerable<LogonInfo> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return new List<LogonInfo>();
		}
	}
}
