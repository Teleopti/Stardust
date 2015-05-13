using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Toggle;
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
				new FakeContainerHolder(),
				false
			);

			jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);

			return jobParameters;
		}

		public class FakeContainerHolder : IContainerHolder
		{
			public FakeContainerHolder()
			{
				ToggleManager = new FakeToggleManager();
				TenantLogonDataManager = new FakeTenantLogonDataManager();
			}
			public IToggleManager ToggleManager { get; private set; }
			public ITenantLogonDataManager TenantLogonDataManager { get; private set; }

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

	public class FakeTenantLogonDataManager : ITenantLogonDataManager
	{
		public IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return new List<LogonInfoModel>();
		}
	}
}
