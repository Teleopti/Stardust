using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Gamification;
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


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Gamification
{
	[TestFixture, DomainTest]
	public class RecalculateBadgeEventHandlerTest:IIsolateSystem
	{
		public RecalculateBadgeEventHandler Target;
		public FakeJobResultRepository JobResultRepository;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public FakePersonRepository PersonRepository;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakePushMessageDialogueRepository PushMessageDialogueRepository;

		private IPerson _agent;
		private ITeam _team;
		private GamificationSetting _gamificationSetting;
		private BadgeSetting _badgeSetting;
		private ITeamGamificationSetting _teamGamificationSetting;

		private readonly int _externalId = 1;
		private readonly DateOnly _calculatedate = new DateOnly(2018, 2, 28);

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<RecalculateBadgeEventHandler>().For<IHandleEvent<RecalculateBadgeEvent>>();
			isolate.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<FakePushMessageDialogueRepository>().For<IPushMessageDialogueRepository>();
			isolate.UseTestDouble<FakePushMessageRepository>().For<IPushMessageRepository>();
			isolate.UseTestDouble<PerformAllBadgeCalculation>().For<IPerformBadgeCalculation>();
		}

		[Test]
		public void ShouldResolveHandler()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCleanOldBadgeData()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("bu"));
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction { CalculatedDate = _calculatedate });
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction() { CalculatedDate = _calculatedate });
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);

			Target.Handle(badgeEvent);
			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}

		[Test]
		public void ShouldUpdateBadgeAmountWhenRecalculate()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 1, period, 87);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate, true);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(1);
			Target.Handle(badgeEvent);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate, true);
			newData.BronzeBadgeAmount.Should().Be.EqualTo(0);
			newData.SilverBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveBadgeWhenNewDataCannotGetBadge()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 1, period, 70);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate, true);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(1);
			Target.Handle(badgeEvent);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate, true);
			newData.Should().Be.Null();
		}

		[Test]
		public void ShouldAddNewBadgeWhenNewDataCanGetABadge()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date.AddDays(-1),
				EndDate = _calculatedate.Date.AddDays(1)
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate, true);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(0);
			Target.Handle(badgeEvent);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate, true);
			newData.BronzeBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCalculateMultiDaysData()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date.AddDays(-1),
				EndDate = _calculatedate.Date.AddDays(1)
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			var oldData = AgentBadgeWithRankTransactionRepository.LoadAll();
			oldData.Count().Should().Be.EqualTo(1);
			Target.Handle(badgeEvent);

			var newData = AgentBadgeWithRankTransactionRepository.LoadAll();
			newData.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldSetJobResultFinishedAfterRecalculated()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			jobResult.FinishedOk.Should().Be.False();
			Target.Handle(badgeEvent);

			jobResult.FinishedOk.Should().Be.True();
		}

		[Test]
		public void ShouldSetJobResultHasErrorWhenErrorHappens()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent { JobResultId = jobResult.Id.GetValueOrDefault() };

			try
			{
				Target.Handle(badgeEvent);
			}
			catch (Exception)
			{
				jobResult.HasError().Should().Be.True();
			}
		}
		
		[Test]
		public void ShouldSendMessageWhenGetBronzeExternalBadge()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 87);

			Target.Handle(badgeEvent);

			var formmater = new NoFormatting();
			var date = _calculatedate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
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
		public void ShouldNotSendMessageWhenNotGetAnyExternalBadge()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithRatioConvertor, 90, 85, 80, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 77);

			Target.Handle(badgeEvent);

			PushMessageDialogueRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldSendMessageWhenGetGoldExternalBadgeWithRank()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 1, 0, period, 91);

			Target.Handle(badgeEvent);

			var formmater = new NormalizeText();
			var date = _calculatedate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
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
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 87);

			Target.Handle(badgeEvent);

			var formmater = new NormalizeText();
			var date = _calculatedate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
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
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 81);

			Target.Handle(badgeEvent);

			var formmater = new NormalizeText();
			var date = _calculatedate.Date.ToString(_agent.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern,
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
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = newJobResult(period);
			var badgeEvent = new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = _calculatedate.Date,
				EndDate = _calculatedate.Date
			};
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 79);

			Target.Handle(badgeEvent);

			PushMessageDialogueRepository.LoadAll().Should().Be.Empty();
		}

		private void createExistingBadgeAndNewData(int gold, int silver, int bronze, DateOnlyPeriod period, int newScore)
		{
			var externalId = 1;
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("bu"));
			var agentBadge = new AgentBadgeWithRankTransaction
			{
				BadgeType = externalId,
				IsExternal = true,
				BronzeBadgeAmount = bronze,
				SilverBadgeAmount = silver,
				GoldBadgeAmount = gold,
				CalculatedDate = _calculatedate,
				Person = PersonFactory.CreatePersonWithId(_agent.Id.GetValueOrDefault())
			};
			AgentBadgeWithRankTransactionRepository.Add(agentBadge);

			var externalPerformance = new ExternalPerformance { ExternalId = externalId };
			foreach (var date in period.DayCollection())
			{
				var externalPerformanceData = new ExternalPerformanceData
				{
					ExternalPerformance = externalPerformance,
					DateFrom = date,
					PersonId = _agent.Id.GetValueOrDefault(),
					Score = newScore
				};
				ExternalPerformanceDataRepository.Add(externalPerformanceData);
			}
		}

		private void createGamificationSetting(GamificationSettingRuleSet rule, double gold, double silver, double bronze, double threshold = 0)
		{
			_gamificationSetting = new GamificationSetting("GamificationSetting1")
			{
				GamificationSettingRuleSet = rule,
				SilverToBronzeBadgeRate = 2,
				GoldToSilverBadgeRate = 2
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

			_teamGamificationSetting = new TeamGamificationSetting
			{
				Team = _team,
				GamificationSetting = _gamificationSetting
			};
			TeamGamificationSettingRepository.Add(_teamGamificationSetting);
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

		private IJobResult newJobResult(DateOnlyPeriod peroid)
		{
			var person = PersonFactory.CreatePerson().WithId();
			var jobResult = new JobResult(JobCategory.WebImportExternalGamification, peroid, person, DateTime.UtcNow).WithId();
			jobResult.SetVersion(1);
			JobResultRepository.Add(jobResult);
			var updated = JobResultRepository.Get(jobResult.Id.GetValueOrDefault());
			return updated;
		}
	}
}
