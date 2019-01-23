using System;
using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PortalViewModelFactoryTest : IIsolateSystem
	{
		public FakeCurrentDatasource CurrentDataSource;
		public IPortalViewModelFactory Target;
		public FakeCurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public CurrentTenantUserFake CurrentTenantUser;
		public MutableNow Now;

		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeGamificationSettingRepository GamificationSettingRepository;
		public FakeToggleManager ToggleManager;
		public FakeCurrentTeleoptiPrincipal CurrentTeleoptiPrincipal;
		public FakePermissionProvider PermissionProvider;
		public FakePermissions Authorization;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<CurrentTenantUserFake>().For<ICurrentTenantUser>();
			isolate.UseTestDouble(new FakeCurrentUnitOfWorkFactory(null).WithCurrent(new FakeUnitOfWorkFactory(null, null, null)
			{
				Name = MyTimeWebTestAttribute.DefaultTenantName
			})).For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			isolate.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
			isolate.UseTestDouble<FakeCurrentDatasource>().For<ICurrentDataSource>();
			isolate.UseTestDouble<FakePermissions>().For<IAuthorization>();
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
		public void ShouldHaveLogonAgentId()
		{
			setupLoggedOnUser();

			var result = Target.CreatePortalViewModel();

			result.CurrentLogonAgentId.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
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
			Now.Is(new DateTime(2017, 04, 09, 23, 0, 0, DateTimeKind.Utc));

			ToggleManager.Disable(Toggles.WFM_Gamification_Create_Rolling_Periods_74866);
			var calculatedDate = new DateOnly(2017, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);
			setAgentBadge(gamificationSetting, calculatedDate);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate);

			var result = Target.CreatePortalViewModel().Badges.ToList();
			result[0].BronzeBadge.Should().Be.EqualTo(1);
			result[0].SilverBadge.Should().Be.EqualTo(0);
			result[0].GoldBadge.Should().Be.EqualTo(0);
			result[1].BronzeBadge.Should().Be.EqualTo(5);
			result[1].SilverBadge.Should().Be.EqualTo(0);
			result[1].GoldBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetDefaultSettingPeriodAgentBadgesWhenToggleOn()
		{
			ToggleManager.Enable(Toggles.WFM_Gamification_Create_Rolling_Periods_74866);
			var calculatedDate = new DateOnly(2017, 4, 9);
			var gamificationSetting = createGamificationSetting();
			createTeamGamificationSetting(gamificationSetting);
			setAgentBadge(gamificationSetting, calculatedDate);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate);

			var result = Target.CreatePortalViewModel().Badges.ToList();
			result[0].BronzeBadge.Should().Be.EqualTo(0);
			result[0].SilverBadge.Should().Be.EqualTo(0);
			result[0].GoldBadge.Should().Be.EqualTo(0);
			result[1].BronzeBadge.Should().Be.EqualTo(0);
			result[1].SilverBadge.Should().Be.EqualTo(0);
			result[1].GoldBadge.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetWeeklyDefaultSettingPeriodAgentBadgesWhenToggleOn()
		{
			var calculatedDate = new DateOnly(2017, 4, 9);

			Now.Is(new DateTime(2017,04,08,23,0,0,DateTimeKind.Utc));
			ToggleManager.Enable(Toggles.WFM_Gamification_Create_Rolling_Periods_74866);

			var gamificationSetting = createWeeklyGamificationSetting();
			createTeamGamificationSetting(gamificationSetting, calculatedDate);
			setAgentBadge(gamificationSetting, calculatedDate);
			setAgentBadgeWithRank(gamificationSetting, calculatedDate);

			var result = Target.CreatePortalViewModel().Badges.ToList();
			result[0].BronzeBadge.Should().Be.EqualTo(1);
			result[0].SilverBadge.Should().Be.EqualTo(0);
			result[0].GoldBadge.Should().Be.EqualTo(0);
			result[1].BronzeBadge.Should().Be.EqualTo(5);
			result[1].SilverBadge.Should().Be.EqualTo(0);
			result[1].GoldBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapNewTeamScheduleMenuItemWhenToggle75989IsOn()
		{
			setupLoggedOnUser();
			setLicense("default");

			ToggleManager.Enable(Toggles.MyTimeWeb_NewTeamScheduleView_75989);

			var result = Target.CreatePortalViewModel().NavigationItems;
			result.FirstOrDefault(r => r.Title == Resources.TeamSchedule)?.Action.Should().Be("NewIndex");
		}

		[TestCase(true, true, true, true)]
		[TestCase(false, true, true, false)]
		[TestCase(true, false, true, false)]
		[TestCase(true, true, false, false)]
		public void ShouldIndicateIfGrantChatBotEnabled(bool licensed, bool hasPermission, bool toggleEnabled,
			bool expectedGrantBotEnabled)
		{
			setupLoggedOnUser();
			setLicense(licensed ? "default" : "");
			PermissionProvider.SetApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ChatBot,
				hasPermission);
			if (toggleEnabled)
			{
				ToggleManager.Enable(Toggles.WFM_ChatBot_77547);
			}
			else
			{
				ToggleManager.Disable(Toggles.WFM_ChatBot_77547);
			}

			var result = Target.CreatePortalViewModel();
			result.GrantEnabled.Should().Be.EqualTo(expectedGrantBotEnabled);
		}

		private void setLicense(string name)
		{
			CurrentDataSource.FakeName(name);
		}

		private void setAgentBadge(IGamificationSetting gamificationSetting, DateOnly calculatedDate)
		{
			var agentBadge = new AgentBadgeTransaction
			{
				BadgeType = gamificationSetting.BadgeSettings[0].QualityId,
				IsExternal = true,
				Person = LoggedOnUser.CurrentUser(),
				Amount = 1,
				CalculatedDate = calculatedDate
			};
			AgentBadgeTransactionRepository.Add(agentBadge);
		}

		private void setAgentBadgeWithRank(IGamificationSetting gamificationSetting, DateOnly calculatedDate)
		{
			var agentBadgeWithRank = new AgentBadgeWithRankTransaction
			{
				BadgeType = gamificationSetting.BadgeSettings[1].QualityId,
				BronzeBadgeAmount = 5,
				SilverBadgeAmount = 0,
				GoldBadgeAmount = 1,
				IsExternal = true,
				CalculatedDate = calculatedDate,
				Person = LoggedOnUser.CurrentUser()
			};

			AgentBadgeWithRankTransactionRepository.Add(agentBadgeWithRank);
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
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("Pelle", null, null, null, null, null), person);
			CurrentTeleoptiPrincipal.Fake(principal);
		}

		private void createTeamGamificationSetting(IGamificationSetting gamificationSetting = null, DateOnly? date = null)
		{
			setupLoggedOnUser();
			if (gamificationSetting == null) return;
		
			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = LoggedOnUser.CurrentUser().MyTeam(date ?? DateOnly.Today),
				GamificationSetting = gamificationSetting
			};
			TeamGamificationSettingRepository.Add(teamGamificationSetting);
		}

		private IGamificationSetting createWeeklyGamificationSetting()
		{
			var gSetting = new GamificationSetting("bla")
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
				RollingPeriodSet = GamificationRollingPeriodSet.Weekly
			};

			gSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 2, DataType = ExternalPerformanceDataType.Numeric, Enabled = true });
			gSetting.BadgeSettings.Add(new BadgeSetting { QualityId = 1, DataType = ExternalPerformanceDataType.Percent, Enabled = true });

			gSetting.WithId(Guid.NewGuid());

			GamificationSettingRepository.Add(gSetting);
			return gSetting;
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