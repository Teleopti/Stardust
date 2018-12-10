using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture, ServiceBusTest]
	public class PerformAllBadgeCalculationTest: IIsolateSystem
	{
		public PerformAllBadgeCalculation Target;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakePersonRepository PersonRepository;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public FakePushMessageDialogueRepository PushMessageDialogueRepository;
		public FakeAgentBadgeRepository AgentBadgeRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeBadgeCalculationRepository BadgeCalculationRepository;

		private ITeamGamificationSetting _teamGamificationSetting;
		private GamificationSetting _gamificationSetting;
		private IPerson _agent;
		private ITeam _team;
		private BadgeSetting _badgeSetting;
		private readonly DateTime _systemCalculateDate = DateTime.UtcNow.AddDays(-2);

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PerformAllBadgeCalculation>().For<IPerformBadgeCalculation>();
			isolate.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
			isolate.UseTestDouble<FakePushMessageDialogueRepository>().For<IPushMessageDialogueRepository>();
			isolate.UseTestDouble<FakeAgentBadgeWithRankTransactionRepository>().For<IAgentBadgeWithRankTransactionRepository>();
			isolate.UseTestDouble<FakeAgentBadgeTransactionRepository>().For<IAgentBadgeTransactionRepository>();
			isolate.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			isolate.UseTestDouble<LogObjectDateChecker>().For<ILogObjectDateChecker>();
			isolate.UseTestDouble<FakeStatisticRepository>().For<IStatisticRepository>();
			isolate.UseTestDouble<FakeBadgeCalculationRepository>().For<IBadgeCalculationRepository>();
		}

		[Test]
		public void ShouldGetGoldExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(90);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.First().IsExternal.Should().Be.True();
			result.First().GoldBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetSilverExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(87);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.First().IsExternal.Should().Be.True();
			result.First().SilverBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetBronzeExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(81);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.First().IsExternal.Should().Be.True();
			result.First().BronzeBadgeAmount.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldNotGetExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(79);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCalcuateExternalBadgeWithRankWhenBadgeSettingNotEnabled()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			_badgeSetting.Enabled = false;
			addExternalPerformanceData(81);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSendMessageWhenGetGoldExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(91);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NormalizeText();
			var date = _systemCalculateDate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
				_agent.PermissionInformation.Culture());
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.YouGotANewGoldBadge, _badgeSetting.Name, date));
		}

		[Test]
		public void ShouldSendMessageWhenGetSilverExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(87);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NormalizeText();
			var date = _systemCalculateDate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
				_agent.PermissionInformation.Culture());
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.YouGotANewSilverBadge, _badgeSetting.Name, date));
		}

		[Test]
		public void ShouldSendMessageWhenGetBronzeExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(81);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NormalizeText();
			var date = _systemCalculateDate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
				_agent.PermissionInformation.Culture());
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.YouGotANewBronzeBadge, _badgeSetting.Name, date));
		}

		[Test]
		public void ShouldNotSendMessageWhenNotGetAnyExternalBadgeWithRank()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			addExternalPerformanceData(79);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);
			PushMessageDialogueRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldGetExternalBadge()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			addExternalPerformanceData(87);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeTransactionRepository.LoadAll();
			result.First().IsExternal.Should().Be.True();
			result.First().Amount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotCalculateExternalBadgeWhenBadgeSettingNotEnabled()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			_badgeSetting.Enabled = false;
			addExternalPerformanceData(87);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetExternalBadge()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			addExternalPerformanceData(79);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSendMessageWhenGetBronzeExternalBadge()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			addExternalPerformanceData(87);

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NoFormatting();
			var date = _systemCalculateDate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
				_agent.PermissionInformation.Culture());
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.YouGotANewBronzeBadge, _badgeSetting.Name, date));
		}

		[Test]
		public void ShouldSendMessageForBothExternalAndInternalBadge()
		{
			createAgentAndTeam();
			createGamificationSettingWithBothInternalAndExternalMeasures(GamificationSettingRuleSet.RuleWithRatioConvertor, new TimeSpan(0, 4, 0), 80);
			addExternalPerformanceData(87);
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAht(_systemCalculateDate, new TimeSpan(0, 3, 0), _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			PushMessageDialogueRepository.LoadAll().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSendMessageWhenGetSilverExternalBadge()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			addExternalPerformanceData(87);
			AgentBadgeRepository.Add(new Domain.Common.AgentBadge(){BadgeType = _badgeSetting.QualityId, IsExternal = true, Person = _agent.Id.GetValueOrDefault(), TotalAmount = 1});

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NoFormatting();
			var date = _systemCalculateDate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
				_agent.PermissionInformation.Culture());
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.YouGotANewSilverBadge, _badgeSetting.Name, date));
		}

		[Test]
		public void ShouldSendMessageWhenGetGoldExternalBadge()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			addExternalPerformanceData(87);
			AgentBadgeRepository.Add(new Domain.Common.AgentBadge(){BadgeType = 1, IsExternal = true, Person = _agent.Id.GetValueOrDefault(), TotalAmount = 3});

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NoFormatting();
			var date = _systemCalculateDate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
				_agent.PermissionInformation.Culture());
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.YouGotANewGoldBadge, _badgeSetting.Name, date));
		}

		[Test]
		public void ShouldNotSendMessageWhenNotGetAnyExternalBadge()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			addExternalPerformanceData(77);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			PushMessageDialogueRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotCalculateExternalBadgeWhenSettingIsDeleted()
		{
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			_gamificationSetting.SetDeleted();
			addExternalPerformanceData(87);

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCalculateSystemBadgeWhenSettingIsDeleted()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AHTBadgeEnabled = true
			};
			_gamificationSetting.SetDeleted();
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCalculateSystemBadgeWhenNoBadgeTypeEnableForAppliedSetting()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotCalculateSystemBadgeWhenNoSettingAppliedToAnyTeam()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AHTBadgeEnabled = true,
				AdherenceBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true
			};
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

			Target.Calculate(Guid.NewGuid(), DateTime.Today);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldCalculateSystemAHTBadgeWithRank()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AHTBadgeEnabled = true
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAht(_systemCalculateDate, new TimeSpan(0, 4, 0), _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.First().SilverBadgeAmount.Should().Be.EqualTo(1);
			result.First().Person.Should().Be.EqualTo(_agent);
		}

		[Test]
		public void ShouldCalculateSystemAnsweredCallsBadgeWithRank()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = true
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAnsweredCalls(_systemCalculateDate, 100, _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeWithRankTransactionRepository.LoadAll();
			result.First().BronzeBadgeAmount.Should().Be.EqualTo(1);
			result.First().Person.Should().Be.EqualTo(_agent);
		}

		[Test]
		public void ShouldCalculateSystemAnsweredCallsBadge()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor,
				SilverToBronzeBadgeRate = 2,
				GoldToSilverBadgeRate = 2,
				AnsweredCallsBadgeEnabled = true
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAnsweredCalls(_systemCalculateDate, 100, _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeTransactionRepository.LoadAll();
			result.First().Amount.Should().Be.EqualTo(1);
			result.First().Person.Should().Be.EqualTo(_agent);
		}

		[Test]
		public void ShouldCalculateSystemAHTBadge()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor,
				SilverToBronzeBadgeRate = 2,
				GoldToSilverBadgeRate = 2,
				AHTBadgeEnabled = true
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAht(_systemCalculateDate, new TimeSpan(0,4,0), _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var result = AgentBadgeTransactionRepository.LoadAll();
			result.First().Amount.Should().Be.EqualTo(1);
			result.First().Person.Should().Be.EqualTo(_agent);
		}

		[Test]
		public void ShouldSendMessageWhenGetAHTBadge()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AHTBadgeEnabled = true
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAht(_systemCalculateDate, new TimeSpan(0,4,0), _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NoFormatting();
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.BadgeWithRank_YouGotANewSilverBadgeForAHT, 
					_gamificationSetting.AHTSilverThreshold.TotalSeconds.ToString(CultureInfo.InvariantCulture),
					_systemCalculateDate));
		}

		[Test]
		public void ShouldSendMessageWhenGetAnsweredCallsBadge()
		{
			createAgentAndTeam();
			_gamificationSetting = new GamificationSetting("GamificationSetting2")
			{
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold,
				AnsweredCallsBadgeEnabled = true
			};
			setTeamGamificationSetting();
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			BadgeCalculationRepository.AddAnsweredCalls(_systemCalculateDate, 160, _agent.Id.GetValueOrDefault());

			Target.Calculate(Guid.NewGuid(), _systemCalculateDate);

			var formmater = new NoFormatting();
			var dialogue = PushMessageDialogueRepository.LoadAll().First();
			var resultMessage = dialogue.PushMessage;
			dialogue.Receiver.Id.Should().Be.EqualTo(_agent.Id.GetValueOrDefault());
			resultMessage.AllowDialogueReply.Should().Be.False();
			resultMessage.GetTitle(formmater).Should().Be.EqualTo(Resources.Congratulations);
			resultMessage.GetMessage(formmater).Should().Be.EqualTo(
				string.Format(Resources.BadgeWithRank_YouGotANewGoldBadgeForAnsweredCalls, 
					_gamificationSetting.AnsweredCallsGoldThreshold.ToString(CultureInfo.InvariantCulture),
					_systemCalculateDate));
		}

		private void setTeamGamificationSetting()
		{
			_teamGamificationSetting = new TeamGamificationSetting
			{
				Team = _team,
				GamificationSetting = _gamificationSetting
			};
			TeamGamificationSettingRepository.Add(_teamGamificationSetting);
		}

		private void createGamificationSettingWithBothInternalAndExternalMeasures(GamificationSettingRuleSet rule, TimeSpan internalThreshold, double threshold)
		{
			_gamificationSetting = new GamificationSetting("GamificationSetting1")
			{
				GamificationSettingRuleSet = rule,
				SilverToBronzeBadgeRate = 2,
				GoldToSilverBadgeRate = 2,
				AHTBadgeEnabled = true,
				AHTThreshold = internalThreshold

			};
			_badgeSetting = new BadgeSetting
			{
				Name = "Performance1",
				Enabled = true,
				QualityId = 1,
				GoldThreshold = 0,
				SilverThreshold = 0,
				BronzeThreshold = 0,
				Threshold = threshold,
				LargerIsBetter = true
			};
			_gamificationSetting.AddBadgeSetting(_badgeSetting);
			setTeamGamificationSetting();
		}

		private void createGamificationSetting(GamificationSettingRuleSet rule, double gold, double silver, double bronze, double threshold = 0)
		{
			_gamificationSetting = new GamificationSetting("GamificationSetting1")
			{
				GamificationSettingRuleSet = rule,
				SilverToBronzeBadgeRate = 2,
				GoldToSilverBadgeRate = 2,
				AHTBadgeEnabled = true
			};
			_badgeSetting = new BadgeSetting
			{
				Name = "Performance1",
				Enabled = true,
				QualityId = 1,
				GoldThreshold = gold,
				SilverThreshold = silver,
				BronzeThreshold = bronze,
				Threshold = threshold,
				LargerIsBetter = true
			};
			_gamificationSetting.AddBadgeSetting(_badgeSetting);
			setTeamGamificationSetting();
		}

		private void addExternalPerformanceData(double score)
		{
			ExternalPerformanceDataRepository.Add(new ExternalPerformanceData()
			{
				DateFrom = new DateOnly(_systemCalculateDate),
				ExternalPerformance = new ExternalPerformance() { ExternalId = 1 },
				PersonId = _agent.Id.GetValueOrDefault(),
				Score = score
			});
		}

		private void createAgentAndTeam()
		{
			_team = TeamFactory.CreateSimpleTeam("myTeam");
			_agent = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.MinValue, _team);
			_agent.WithId(Guid.NewGuid());
			_agent.PermissionInformation.SetCulture(CultureInfo.CreateSpecificCulture("sv-SE"));
			var applicationFunction = ApplicationFunction.FindByPath(
				new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.ViewBadge);
			var applicationRole = ApplicationRoleFactory.CreateRole("roleName", "roleName");
			applicationRole.AddApplicationFunction(applicationFunction);
			_agent.PermissionInformation.AddApplicationRole(applicationRole);
			PersonRepository.Add(_agent);
		}
	}
}
