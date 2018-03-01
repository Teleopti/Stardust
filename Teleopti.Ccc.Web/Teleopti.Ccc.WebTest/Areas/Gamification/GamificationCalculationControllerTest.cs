using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[GamificationTest]
	[TestFixture]
	public class GamificationCalculationControllerTest
	{
		public GamificationCalculationController Target;
		public FakeAgentBadgeWithRankTransactionRepository AgentBadgeWithRankTransactionRepository;
		public FakeAgentBadgeTransactionRepository AgentBadgeTransactionRepository;
		public IPerformBadgeCalculation PerformBadgeCalculation;
		public FakeExternalPerformanceDataRepository ExternalPerformanceDataRepository;
		public FakePersonRepository PersonRepository;
		public FakeTeamGamificationSettingRepository TeamGamificationSettingRepository;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;

		private IPerson _agent;
		private ITeam _team;
		private GamificationSetting _gamificationSetting;
		private BadgeSetting _badgeSetting;
		private ITeamGamificationSetting _teamGamificationSetting;

		private readonly int _externalId = 1;
		private readonly DateOnly _calculatedate = new DateOnly(2018, 2, 28);

		[Test]
		public void ShouldResetAgentBadges()
		{
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction());
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction());

			Target.ResetBadge();

			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}

		[Test]
		public void ShouldCleanOldBadgeData()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("bu"));
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction{CalculatedDate = _calculatedate});
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction(){CalculatedDate = _calculatedate});
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);

			Target.RecalculateBadges(period);
			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}

		[Test]
		public void ShouldUpdateBadgeAmountWhenRecalculate()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 1, period, 87);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(1);
			Target.RecalculateBadges(period);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			newData.BronzeBadgeAmount.Should().Be.EqualTo(0);
			newData.SilverBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveBadgeWhenNewDataCannotGetBadge()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 1, period, 70);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(1);
			Target.RecalculateBadges(period);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			newData.Should().Be.Null();
		}

		[Test]
		public void ShouldAddNewBadgeWhenNewDataCanGetABadge()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(0);
			Target.RecalculateBadges(period);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			newData.BronzeBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCalculateMultiDaysData()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			var oldData = AgentBadgeWithRankTransactionRepository.LoadAll();
			oldData.Count().Should().Be.EqualTo(1);
			Target.RecalculateBadges(period);

			var newData = AgentBadgeWithRankTransactionRepository.LoadAll();
			newData.Count().Should().Be.EqualTo(3);
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

			var externalPerformance = new ExternalPerformance {ExternalId = externalId};
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
	}
}
