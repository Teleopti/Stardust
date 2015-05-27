﻿using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
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


	public class FakeTenantLogonInfoLoader : ITenantLogonInfoLoader
	{
		public IEnumerable<LogonInfo> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return new List<LogonInfo>();
		}
	}

}
