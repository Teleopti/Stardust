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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Gamification
{
	[TestFixture, DomainTest]
	public class RecalculateBadgeHandlerTest:ISetup
	{
		public RecalculateBadgeHandler Target;
		public FakeJobResultRepository JobResultRepository;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<RecalculateBadgeHandler>().For<IHandleEvent<RecalculateBadgeEvent>>();
			system.UseTestDouble<FakeJobResultRepository>().For<IJobResultRepository>();
			system.UseTestDouble<FakeAgentBadgeWithRankTransactionRepository>().For<IAgentBadgeWithRankTransactionRepository>();
			system.UseTestDouble<FakeAgentBadgeTransactionRepository>().For<IAgentBadgeTransactionRepository>();
			system.UseTestDouble<PerformAllBadgeCalculation>().For<IPerformBadgeCalculation>();
			system.UseTestDouble<FakeCurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			system.UseTestDouble<GamificationSetting>().For<IGamificationSetting>();
			system.UseTestDouble<BadgeSetting>().For<IBadgeSetting>();
			system.UseTestDouble<FakeTeamGamificationSettingRepository>().For<ITeamGamificationSettingRepository>();
			system.UseTestDouble<FakeExternalPerformanceDataRepository>().For<IExternalPerformanceDataRepository>();
			system.UseTestDouble<PushMessageRepository>().For<IPushMessageRepository>();
			system.UseTestDouble<FakePushMessagePersister>().For<IPushMessagePersister>();
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
			var jobResult = NewJobResult();
			var message = new RecalculateBadgeEvent {Period = period , JobResultId = jobResult.Id.GetValueOrDefault() };
			CurrentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("bu"));
			AgentBadgeTransactionRepository.Add(new AgentBadgeTransaction { CalculatedDate = _calculatedate });
			AgentBadgeWithRankTransactionRepository.Add(new AgentBadgeWithRankTransaction() { CalculatedDate = _calculatedate });
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);

			Target.Handle(message);
			AgentBadgeTransactionRepository.LoadAll().Any().Should().Be.False();
			AgentBadgeWithRankTransactionRepository.LoadAll().Any().Should().Be.False();
		}

		[Test]
		public void ShouldUpdateBadgeAmountWhenRecalculate()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = NewJobResult();
			var message = new RecalculateBadgeEvent { Period = period, JobResultId = jobResult.Id.GetValueOrDefault() };
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 1, period, 87);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(1);
			Target.Handle(message);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			newData.BronzeBadgeAmount.Should().Be.EqualTo(0);
			newData.SilverBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRemoveBadgeWhenNewDataCannotGetBadge()
		{
			var period = new DateOnlyPeriod(_calculatedate, _calculatedate);
			var jobResult = NewJobResult();
			var message = new RecalculateBadgeEvent { Period = period, JobResultId = jobResult.Id.GetValueOrDefault() };
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 1, period, 70);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(1);
			Target.Handle(message);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			newData.Should().Be.Null();
		}

		[Test]
		public void ShouldAddNewBadgeWhenNewDataCanGetABadge()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			var jobResult = NewJobResult();
			var message = new RecalculateBadgeEvent { Period = period, JobResultId = jobResult.Id.GetValueOrDefault() };
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			var oldData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			oldData.BronzeBadgeAmount.Should().Be.EqualTo(0);
			Target.Handle(message);

			var newData = AgentBadgeWithRankTransactionRepository.Find(_agent, _externalId, _calculatedate);
			newData.BronzeBadgeAmount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCalculateMultiDaysData()
		{
			var period = new DateOnlyPeriod(_calculatedate.AddDays(-1), _calculatedate.AddDays(1));
			var jobResult = NewJobResult();
			var message = new RecalculateBadgeEvent { Period = period, JobResultId = jobResult.Id.GetValueOrDefault() };
			createAgentAndTeam();
			createGamificationSetting(GamificationSettingRuleSet.RuleWithDifferentThreshold, 90, 85, 80);
			createExistingBadgeAndNewData(0, 0, 0, period, 80);

			var oldData = AgentBadgeWithRankTransactionRepository.LoadAll();
			oldData.Count().Should().Be.EqualTo(1);
			Target.Handle(message);

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

		private IJobResult NewJobResult()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var jobResult = new JobResult(JobCategory.WebImportExternalGamification, DateOnly.Today.ToDateOnlyPeriod(), person, DateTime.UtcNow).WithId();
			jobResult.SetVersion(1);
			JobResultRepository.Add(jobResult);
			var updated = JobResultRepository.Get(jobResult.Id.Value);
			return updated;
		}
	}
}
