using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class CalculateBadgesTest
	{
		private const int defaultDatabaseTimeout = 60;
		private ITeamGamificationSettingRepository teamSettingsRepository;
		private IAgentBadgeRepository badgeRepository;
		private IAgentBadgeWithRankRepository badgeWithRankRepository;
		private IPersonRepository personRepository;
		private IPushMessagePersister msgRepository;
		private CalculateBadges target;
		private IAgentBadgeCalculator calculator;
		private IAgentBadgeWithRankCalculator badgeWithRankCalculator;
		private INow now;
		private IUnitOfWorkFactory loggedOnUnitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IRunningEtlJobChecker etlJobChecker;
		private IGlobalSettingDataRepository globalSettingRep;
		private Guid _businessUnitId;

		[SetUp]
		public void Setup()
		{
			loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			loggedOnUnitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			teamSettingsRepository = MockRepository.GenerateMock<ITeamGamificationSettingRepository>();

			badgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			badgeWithRankRepository = MockRepository.GenerateMock<IAgentBadgeWithRankRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();

			personRepository.Stub(
				x => x.FindPeopleInOrganization(new DateOnlyPeriod(new DateOnly(2014, 8, 7), new DateOnly(2014, 8, 9)), false))
				.Return(new List<IPerson> { new Person() });

			msgRepository = MockRepository.GenerateMock<IPushMessagePersister>();

			globalSettingRep = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			globalSettingRep.Stub(x => x.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()))
				.IgnoreArguments()
				.Return(new AdherenceReportSetting());
			now = MockRepository.GenerateMock<INow>();

			_businessUnitId = new Guid();
			// Mock badge calculator
			calculator = MockRepository.GenerateMock<IAgentBadgeCalculator>();
			calculator.Stub(x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting("test"), _businessUnitId, defaultDatabaseTimeout)).IgnoreArguments()
				.Return(new List<IAgentBadgeTransaction>());
			calculator.Stub(x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				new GamificationSetting("test"), _businessUnitId, defaultDatabaseTimeout)).IgnoreArguments()
				.Return(new List<IAgentBadgeTransaction>());
			calculator.Stub(x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting("test"), _businessUnitId, defaultDatabaseTimeout)).IgnoreArguments()
				.Return(new List<IAgentBadgeTransaction>());

			// Mock badge with rank calculator
			badgeWithRankCalculator = MockRepository.GenerateMock<IAgentBadgeWithRankCalculator>();

			badgeWithRankCalculator.Stub(
				x =>
					x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today, AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, new GamificationSetting("test"),
						_businessUnitId, defaultDatabaseTimeout)).Return(new List<IAgentBadgeWithRankTransaction>()).IgnoreArguments();

			badgeWithRankCalculator.Stub(
				x =>
					x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today, new GamificationSetting("test"),
						_businessUnitId, defaultDatabaseTimeout)).Return(new List<IAgentBadgeWithRankTransaction>()).IgnoreArguments();

			etlJobChecker = MockRepository.GenerateMock<IRunningEtlJobChecker>();
			etlJobChecker.Stub(x => x.NightlyEtlJobStillRunning()).Return(false);

			target = new CalculateBadges(teamSettingsRepository, msgRepository, calculator, badgeWithRankCalculator,
				badgeRepository, badgeWithRankRepository, globalSettingRep, personRepository);
		}

		[Test]
		public void ShouldCalculateBadge()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateTime().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> { new Person() };
			personRepository.Stub(
				x =>
					x.FindPeopleBelongTeam(team, new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1))))
				.Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[]
				{
					new TeamGamificationSetting
					{
						Team = team,
						GamificationSetting = newSetting
					}
				});

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			calculator.AssertWasCalled(
				x => x.CalculateAHTBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId),
					Arg<int>.Is.Equal(defaultDatabaseTimeout)));

			calculator.AssertWasCalled(
				x => x.CalculateAdherenceBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<AdherenceReportSettingCalculationMethod>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId),
					Arg<int>.Is.Equal(defaultDatabaseTimeout)));

			calculator.AssertWasCalled(
				x => x.CalculateAnsweredCallsBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId),
					Arg<int>.Is.Equal(defaultDatabaseTimeout)));
		}

		[Test]
		public void ShouldCalculateBadgeWithTimeoutSettingFromConfig()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateTime().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> { new Person() };
			personRepository.Stub(
				x =>
					x.FindPeopleBelongTeam(team, new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1))))
				.Return(persons);

			const int timeoutSetting = 1234;
			ConfigurationManager.AppSettings["databaseTimeout"] = timeoutSetting.ToString();

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[]
				{
					new TeamGamificationSetting
					{
						Team = team,
						GamificationSetting = newSetting
					}
				});

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			var newTarget = new CalculateBadges(teamSettingsRepository, msgRepository, calculator, badgeWithRankCalculator,
				badgeRepository, badgeWithRankRepository, globalSettingRep, personRepository);
			newTarget.Calculate(message);

			calculator.AssertWasCalled(
				x => x.CalculateAHTBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId),
					Arg<int>.Is.Equal(timeoutSetting)));

			calculator.AssertWasCalled(
				x => x.CalculateAdherenceBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<AdherenceReportSettingCalculationMethod>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId),
					Arg<int>.Is.Equal(timeoutSetting)));

			calculator.AssertWasCalled(
				x => x.CalculateAnsweredCallsBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId),
					Arg<int>.Is.Equal(timeoutSetting)));
		}

		[Test]
		public void ShouldSendCalculateBadgeMessageAtRightTime()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);

			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = true
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateTime().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> { new Person() };
			personRepository.Stub(
				x =>
					x.FindPeopleBelongTeam(team, new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1))))
				.Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] { new TeamGamificationSetting { Team = team, GamificationSetting = newSetting } });

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeWithRankCalculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(persons, "", DateOnly.Today, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());

			badgeWithRankCalculator.AssertWasCalled(
				x => x.CalculateAdherenceBadges(persons, "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());

			badgeWithRankCalculator.AssertWasCalled(
				x => x.CalculateAnsweredCallsBadges(persons, "", DateOnly.Today, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenAppliedSettingIsDeleted()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			var deletedSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = true,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};
			deletedSetting.SetDeleted();

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateTime().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> { new Person() };
			personRepository.Stub(
				x =>
					x.FindPeopleBelongTeam(team, new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1))))
				.Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] { new TeamGamificationSetting { Team = team, GamificationSetting = deletedSetting } });

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(persons, "", DateOnly.Today, deletedSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());

			calculator.AssertWasNotCalled(
				x => x.CalculateAdherenceBadges(persons, "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, deletedSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());

			calculator.AssertWasNotCalled(
				x => x.CalculateAnsweredCallsBadges(persons, "", DateOnly.Today, deletedSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenNoSettingAppliedToAnyTeam()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);
			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = false,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new List<TeamGamificationSetting>());

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateTime().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenNoBadgeTypeEnableForAppliedSetting()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);

			now.Stub(x => x.UtcDateTime()).Return(today);
			var newSetting = new GamificationSetting("test")
			{
				AdherenceBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = false,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] { new TeamGamificationSetting { Team = team, GamificationSetting = newSetting } });

			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateTime().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			calculator.AssertWasNotCalled(
				x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
					AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
			calculator.AssertWasNotCalled(
				x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today, newSetting, _businessUnitId, defaultDatabaseTimeout),
				o => o.IgnoreArguments());
		}
	}
}