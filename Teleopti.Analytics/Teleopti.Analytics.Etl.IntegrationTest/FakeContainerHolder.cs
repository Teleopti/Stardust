using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
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
	

	public class FakeTenantLogonDataManager : ITenantLogonDataManager
	{
		public List<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return new List<LogonInfoModel>();
		}
	}

}
