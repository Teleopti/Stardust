﻿using System.Configuration;
using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

			system.AddModule(new WfmAdminModule2(configuration));
			system.UseTestDouble<ConsoleLogger>().For<IUpgradeLog>();
			system.UseTestDouble<FakeHangfireCookie>().For<IHangfireCookie>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();

			_tenantUnitOfWorkManager = TenantUnitOfWorkForTest();
			system.AddService(_tenantUnitOfWorkManager);

			system.AddService<DbPathProviderFake>();
			system.AddService<CheckPasswordStrengthFake>();
			system.AddService<TestPollutionCleaner>();
			system.AddService<LoadAllTenants>();
		}

		public static TenantUnitOfWorkManager TenantUnitOfWorkForTest()
		{
			return TenantUnitOfWorkManager.Create(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_tenantUnitOfWorkManager.CancelAndDisposeCurrent();
		}
	}
}