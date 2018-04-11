using System;
using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PortalViewModelFactoryTest : ISetup
	{
		public FakeCurrentDatasource CurrentDataSource;
		public IPortalViewModelFactory Target;
		public FakeCurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public CurrentTenantUserFake CurrentTenantUser;

		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeGamificationSettingRepository GamificationSettingRepository;
		public FakeAgentBadgeRepository AgentBadgeRepository;
		public FakeAgentBadgeWithRankRepository AgentBadgeWithRankRepository;
		public FakeToggleManager ToggleManager;
		public FakeCurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public FakePermissionProvider PermissionProvider;
		public FakePermissions Authorization;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<PrincipalAuthorization>().For<IAuthorization>();
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			system.UseTestDouble(new FakeCurrentUnitOfWorkFactory(null).WithCurrent(new FakeUnitOfWorkFactory(null, null, null, null) { Name = MyTimeWebTestAttribute.DefaultTenantName })).For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			system.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
			system.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			system.UseTestDouble<FakePermissions>().For<IAuthorization>();
		}

		[Test]
		public void ShouldHaveNavigationItems()
		{
			setupLoggedOnUser();
			Authorization.HasPermission(DefinedRaptorApplicationFunctionPaths.MyReportWeb);

			var result = Target.CreatePortalViewModel();

			result.NavigationItems.Should().Have.Count.EqualTo(7);
			result.NavigationItems.ElementAt(0).Action.Should().Be("Week");
			result.NavigationItems.ElementAt(0).Controller.Should().Be("Schedule");
			result.NavigationItems.ElementAt(1).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(1).Controller.Should().Be("TeamSchedule");
			result.NavigationItems.ElementAt(2).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(2).Controller.Should().Be("Availability");
			result.NavigationItems.ElementAt(3).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(3).Controller.Should().Be("Preference");
			result.NavigationItems.ElementAt(4).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(4).Controller.Should().Be("Requests");
			result.NavigationItems.ElementAt(5).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(5).Controller.Should().Be("Message");
			result.NavigationItems.ElementAt(6).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(6).Controller.Should().Be("MyReport");
		}

		[Test]
		public void ShouldHaveMessageNavigationItemsWhenHavingBothLicenseAndPermission()
		{
			setupLoggedOnUser();

			var result = Target.CreatePortalViewModel();

			result.NavigationItems.Should().Have.Count.EqualTo(6);
			result.NavigationItems.ElementAt(5).Action.Should().Be("Index");
			result.NavigationItems.ElementAt(5).Controller.Should().Be("Message");
		}

		[Test]
		public void ShouldHaveCustomerName()
		{
			setupLoggedOnUser();
			setLicense("default");

			var result = Target.CreatePortalViewModel();

			result.CustomerName.Should().Be.EqualTo("Teleopti_RD");
		}

		[Test]
		public void ShouldHaveLogonAgentName()
		{
			setupLoggedOnUser();

			var result = Target.CreatePortalViewModel();

			result.CurrentLogonAgentName.Should().Be.EqualTo("arne arne");
		}

		[Test]
		public void ShouldHideChangePasswordIfNoApplicationLogonExists()
		{
			setupLoggedOnUser();

			var res = Target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.False();
		}

		[Test]
		public void ShouldShowChangePasswordIfApplicationLogonExists()
		{
			setupLoggedOnUser();
			var tenant = new Tenant("Tenn");
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "Perra", "passadej", new OneWayEncryption());
			CurrentTenantUser.Set(personInfo);

			var res = Target.CreatePortalViewModel();
			res.ShowChangePassword.Should().Be.True();
		}

		[Test]
		public void ShouldMapAsmEnabledToTrueWhenHavingBothLicenseAndPermission()
		{
			setupLoggedOnUser();

			var res = Target.CreatePortalViewModel();

			Assert.That(res.AsmEnabled, Is.True);
		}

		[Test]
		public void ShouldMapAsmEnabledToFalseWhenNoAsmLicense()
		{
			setupLoggedOnUser();
			setLicense("");

			var res = Target.CreatePortalViewModel();

			Assert.That(res.AsmEnabled, Is.False);
		}

		[Test]
		public void ShouldMapAsmEnabledToFalseWhenNoAsmPermission()
		{
			PermissionProvider.SetApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger, false);
			setupLoggedOnUser();
			setLicense("default");

			var res = Target.CreatePortalViewModel();

			Assert.That(res.AsmEnabled, Is.False);
		}

		[Test]
		public void ShouldNotShowBadgeIfNoPermission()
		{
			setupLoggedOnUser();
			Assert.That(Target.CreatePortalViewModel().ShowBadge, Is.False);
		}

		[Test, SetCulture("en-US")]
		public void ShouldShowMeridianWhenUsCulture()
		{
			setupLoggedOnUser();
			Target.CreatePortalViewModel().ShowMeridian.Should().Be.True();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldShowMeridianWhenSwedishCulture()
		{
			setupLoggedOnUser();
			Target.CreatePortalViewModel().ShowMeridian.Should().Be.False();
		}

		[Test, SetCulture("en-GB")]
		public void ShouldShowMeridianWhenBritishCulture()
		{
			setupLoggedOnUser();
			Target.CreatePortalViewModel().ShowMeridian.Should().Be.False();
		}

		[Test]
		public void ShouldMapGamificationRollingPeriodSet()
		{
			setupLoggedOnUser();
			Target.CreatePortalViewModel().BadgeRollingPeriodSet.Should().Be.EqualTo(GamificationRollingPeriodSet.OnGoing);
		}

		[Test]
		public void ShouldMapDateFormatLocale()
		{
			setupLoggedOnUser();
			
			Target.CreatePortalViewModel().DateFormatLocale.Should().Be.EqualTo("zh-CN");
		}

		[Test]
		public void ShouldGetOngoingPeriodAgentBadgesWhenToggleOff()
		{
			ToggleManager.Disable(Toggles.WFM_Gamification_Create_Rolling_Periods_74866);
			var calculatedDate = new DateOnly(2017, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);
			setAgentBadge(gamificationSetting, calculatedDate.Date);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate.Date);

			var result = Target.CreatePortalViewModel().Badges;
			result.ToList()[0].BronzeBadge.Should().Be.EqualTo(1);
			result.ToList()[0].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[0].GoldBadge.Should().Be.EqualTo(0);
			result.ToList()[1].BronzeBadge.Should().Be.EqualTo(5);
			result.ToList()[1].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[1].GoldBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetDefaultSettingPeriodAgentBadgesWhenToggleOn()
		{
			ToggleManager.Enable(Toggles.WFM_Gamification_Create_Rolling_Periods_74866);
			var calculatedDate = new DateOnly(2017, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);
			setAgentBadge(gamificationSetting, calculatedDate.Date);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate.Date);

			var result = Target.CreatePortalViewModel().Badges;
			result.ToList()[0].BronzeBadge.Should().Be.EqualTo(0);
			result.ToList()[0].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[0].GoldBadge.Should().Be.EqualTo(0);
			result.ToList()[1].BronzeBadge.Should().Be.EqualTo(0);
			result.ToList()[1].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[1].GoldBadge.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetBadgesWithinPeriod()
		{
			var calculatedDate = new DateOnly(2018, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);

			setAgentBadge(gamificationSetting, calculatedDate.Date);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate.Date);

			var period = new DateOnlyPeriod(DateOnly.MinValue, calculatedDate);
			var result = Target.GetBadges(period);

			result.ToList()[0].BronzeBadge.Should().Be.EqualTo(1);
			result.ToList()[0].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[0].GoldBadge.Should().Be.EqualTo(0);
			result.ToList()[1].BronzeBadge.Should().Be.EqualTo(5);
			result.ToList()[1].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[1].GoldBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotGetBadgesWithoutPeriod()
		{
			var calculatedDate = new DateOnly(2018, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);
			setAgentBadge(gamificationSetting, calculatedDate.Date);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate.Date);

			var period = new DateOnlyPeriod(DateOnly.MinValue, calculatedDate.AddDays(-1));
			var result = Target.GetBadges(period);

			result.ToList()[0].BronzeBadge.Should().Be.EqualTo(0);
			result.ToList()[0].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[0].GoldBadge.Should().Be.EqualTo(0);
			result.ToList()[1].BronzeBadge.Should().Be.EqualTo(0);
			result.ToList()[1].SilverBadge.Should().Be.EqualTo(0);
			result.ToList()[1].GoldBadge.Should().Be.EqualTo(0);
		}

		private void setLicense(string name)
		{
			CurrentDataSource.FakeName(name);
		}

		private void setAgentBadge(IGamificationSetting gamificationSetting, DateTime calculatedDate)
		{
			var agentBadge = new AgentBadge
			{
				BadgeType = gamificationSetting.BadgeSettings[0].QualityId,
				IsExternal = true,
				Person = LoggedOnUser.CurrentUser().Id.GetValueOrDefault(),
				TotalAmount = 1,
				LastCalculatedDate = calculatedDate.Date
			};
			AgentBadgeRepository.Add(agentBadge);
		}

		private void setAgentBadgeWithRank(IGamificationSetting gamificationSetting, DateTime calculatedDate)
		{
			var agentBadgeWithRank = new AgentBadgeWithRank
			{
				BadgeType = gamificationSetting.BadgeSettings[1].QualityId,
				BronzeBadgeAmount = 5,
				SilverBadgeAmount = 0,
				GoldBadgeAmount = 1,
				IsExternal = true,
				LastCalculatedDate = calculatedDate.Date,
				Person = LoggedOnUser.CurrentUser().Id.GetValueOrDefault()
			};

			AgentBadgeWithRankRepository.Add(agentBadgeWithRank);
		}

		private void setupLoggedOnUser()
		{
			CurrentDataSource.FakeName("default");
			var team = TeamFactory.CreateTeamWithId("myTeam");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.MinValue, team);
			person.SetName(new Name("arne", "arne"));
			var culture = CultureInfoFactory.CreateChineseCulture();
			person.PermissionInformation.SetCulture(culture);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null), person);
			CurrentTeleoptiPrincipal.Fake(principal);
		}

		private void createTeamGamificationSetting(IGamificationSetting gamificationSetting = null)
		{
			setupLoggedOnUser();
			if (gamificationSetting == null) return;
		
			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = LoggedOnUser.CurrentUser().MyTeam(DateOnly.Today),
				GamificationSetting = gamificationSetting
			};
			TeamGamificationSettingRepository.Add(teamGamificationSetting);
		}

		private IGamificationSetting createGamificationSetting()
		{
			var gSetting =  new GamificationSetting("bla")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AdherenceBadgeEnabled = false,

				AnsweredCallsThreshold = 30,
				AnsweredCallsBronzeThreshold = 30,
				AnsweredCallsSilverThreshold = 40,
				AnsweredCallsGoldThreshold = 50,

				AHTThreshold = TimeSpan.FromSeconds(60),
				AHTBronzeThreshold = TimeSpan.FromSeconds(60),
				AHTSilverThreshold = TimeSpan.FromSeconds(50),
				AHTGoldThreshold = TimeSpan.FromSeconds(40),

				AdherenceThreshold = new Percent(0.70),
				AdherenceBronzeThreshold = new Percent(0.70),
				AdherenceSilverThreshold = new Percent(0.85),
				AdherenceGoldThreshold = new Percent(0.95),

				SilverToBronzeBadgeRate = 5,
				GoldToSilverBadgeRate = 2,
				RollingPeriodSet = GamificationRollingPeriodSet.Monthly
			};

			gSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 2, DataType = ExternalPerformanceDataType.Numeric, Enabled = true});
			gSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 1, DataType = ExternalPerformanceDataType.Percent, Enabled = true});

			gSetting.WithId(Guid.NewGuid());

			GamificationSettingRepository.Add(gSetting);
			return gSetting;
		}
	}
}