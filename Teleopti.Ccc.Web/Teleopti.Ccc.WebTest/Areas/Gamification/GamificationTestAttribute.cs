﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	class GamificationTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var principalAuthorization = new FullPermission();
			CurrentAuthorization.DefaultTo(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));
			
			system.AddService<FakeStorage>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble(principalAuthorization).For<IAuthorization>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<FakeGamificationSettingRepository>().For<IGamificationSettingRepository>();
			system.UseTestDouble<FakeTeamGamificationSettingRepository>().For<ITeamGamificationSettingRepository>();
			system.UseTestDouble<FakeExternalPerformanceRepository>().For<IExternalPerformanceRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble<FakeAgentBadgeTransactionRepository>().For<IAgentBadgeTransactionRepository>();
			system.UseTestDouble<FakeAgentBadgeWithRankTransactionRepository>().For<IAgentBadgeWithRankTransactionRepository>();
		}
	}
}