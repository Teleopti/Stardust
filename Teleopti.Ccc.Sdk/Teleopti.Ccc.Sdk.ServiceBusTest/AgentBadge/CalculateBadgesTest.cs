using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
		private const string settingNameForTest = "test";
		private ITeamGamificationSettingRepository teamSettingsRepository;
		private IAgentBadgeRepository badgeRepository;
		private IAgentBadgeWithRankRepository badgeWithRankRepository;
		private IPersonRepository personRepository;
		private IPushMessagePersister msgRepository;
		private CalculateBadges target;
		private IAgentBadgeCalculator badgeCalculator;
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

			var period = new DateOnlyPeriod(new DateOnly(2014, 8, 7), new DateOnly(2014, 8, 9));
			personRepository.Stub(x => x.FindPeopleInOrganization(period, false))
				.Return(new List<IPerson> {new Person()});

			msgRepository = MockRepository.GenerateMock<IPushMessagePersister>();

			globalSettingRep = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			globalSettingRep.Stub(x => x.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()))
				.IgnoreArguments()
				.Return(new AdherenceReportSetting());
			now = MockRepository.GenerateMock<INow>();

			_businessUnitId = new Guid();
			// Mock badge calculator
			badgeCalculator = MockRepository.GenerateMock<IAgentBadgeCalculator>();
			badgeCalculator.Stub(x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId))
				.IgnoreArguments().Return(new List<IAgentBadgeTransaction>());
			badgeCalculator.Stub(x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				new GamificationSetting(settingNameForTest), _businessUnitId))
				.IgnoreArguments().Return(new List<IAgentBadgeTransaction>());
			badgeCalculator.Stub(x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId))
				.IgnoreArguments().Return(new List<IAgentBadgeTransaction>());

			// Mock badge with rank calculator
			badgeWithRankCalculator = MockRepository.GenerateMock<IAgentBadgeWithRankCalculator>();
			badgeWithRankCalculator.Stub(x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId))
				.IgnoreArguments().Return(new List<IAgentBadgeWithRankTransaction>());
			badgeWithRankCalculator.Stub(x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				new GamificationSetting(settingNameForTest), _businessUnitId))
				.IgnoreArguments().Return(new List<IAgentBadgeWithRankTransaction>());

			badgeWithRankCalculator.Stub(x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId))
				.IgnoreArguments().Return(new List<IAgentBadgeWithRankTransaction>());

			etlJobChecker = MockRepository.GenerateMock<IRunningEtlJobChecker>();
			etlJobChecker.Stub(x => x.NightlyEtlJobStillRunning()).Return(false);

			target = new CalculateBadges(teamSettingsRepository, msgRepository, badgeCalculator, badgeWithRankCalculator,
				badgeRepository, badgeWithRankRepository, globalSettingRep, personRepository);
		}

		[Test]
		public void ShouldCalculateBadge()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting(settingNameForTest)
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = team,
				GamificationSetting = newSetting
			};

			var calculationDate = TimeZoneInfo.ConvertTime(now.ServerDateTime_DontUse().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> {new Person()};
			var period = new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1));
			personRepository.Stub(x =>x.FindPeopleBelongTeam(team, period)).Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam()).Return(new[] {teamGamificationSetting});

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeCalculator.AssertWasCalled(
				x => x.CalculateAHTBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId)));

			badgeCalculator.AssertWasCalled(
				x => x.CalculateAdherenceBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<AdherenceReportSettingCalculationMethod>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId)));

			badgeCalculator.AssertWasCalled(
				x => x.CalculateAnsweredCallsBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId)));

			badgeWithRankCalculatorShouldNotBeCalledAtAll();
		}

		[Test]
		public void ShouldCalculateBadgeWithRank()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting(settingNameForTest)
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithDifferentThreshold
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = team,
				GamificationSetting = newSetting
			};

			var calculationDate = TimeZoneInfo.ConvertTime(now.ServerDateTime_DontUse().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> { new Person() };
			var period = new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1));
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, period)).Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] { teamGamificationSetting });

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeCalculatorShouldNotBeCalledAtAll();

			badgeWithRankCalculator.AssertWasCalled(
				x => x.CalculateAHTBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId)));

			badgeWithRankCalculator.AssertWasCalled(
				x => x.CalculateAdherenceBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<AdherenceReportSettingCalculationMethod>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId)));

			badgeWithRankCalculator.AssertWasCalled(
				x => x.CalculateAnsweredCallsBadges(
					Arg<IEnumerable<IPerson>>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<DateOnly>.Is.Anything,
					Arg<IGamificationSetting>.Is.Equal(newSetting),
					Arg<Guid>.Is.Equal(_businessUnitId)));
		}

		[Test]
		public void ShouldSendCalculateBadgeMessageAtRightTime()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);

			now.Stub(x => x.UtcDateTime()).Return(today);

			var newSetting = new GamificationSetting(settingNameForTest)
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = true
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var calculationDate = TimeZoneInfo.ConvertTime(now.ServerDateTime_DontUse().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> {new Person()};
			var period = new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1));
			personRepository.Stub(x =>x.FindPeopleBelongTeam(team, period)).Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] {new TeamGamificationSetting {Team = team, GamificationSetting = newSetting}});

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeCalculatorShouldNotBeCalledAtAll();

			badgeWithRankCalculator.AssertWasNotCalled(x => x.CalculateAHTBadges(persons, "",
				DateOnly.Today, newSetting, _businessUnitId),
				o => o.IgnoreArguments());

			badgeWithRankCalculator.AssertWasCalled(x => x.CalculateAdherenceBadges(persons, "",
				DateOnly.Today, AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				newSetting, _businessUnitId),
				o => o.IgnoreArguments());

			badgeWithRankCalculator.AssertWasCalled(x => x.CalculateAnsweredCallsBadges(persons, "",
				DateOnly.Today, newSetting, _businessUnitId),
				o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenAppliedSettingIsDeleted()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			var deletedSetting = new GamificationSetting(settingNameForTest)
			{
				AdherenceBadgeEnabled = true,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = true,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};
			deletedSetting.SetDeleted();

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = team,
				GamificationSetting = deletedSetting
			};

			var calculationDate = TimeZoneInfo.ConvertTime(now.ServerDateTime_DontUse().AddDays(-1), TimeZoneInfo.Local, timezone);
			var calculationDateOnly = new DateOnly(calculationDate);

			var persons = new List<IPerson> {new Person()};
			var period = new DateOnlyPeriod(calculationDateOnly.AddDays(-1), calculationDateOnly.AddDays(1));
			personRepository.Stub(x => x.FindPeopleBelongTeam(team, period)).Return(persons);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] {teamGamificationSetting});

			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeCalculatorShouldNotBeCalledAtAll();
			badgeWithRankCalculatorShouldNotBeCalledAtAll();
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenNoSettingAppliedToAnyTeam()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			now.Stub(x => x.UtcDateTime()).Return(today);

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new List<TeamGamificationSetting>());

			var calculationDate = TimeZoneInfo.ConvertTime(now.ServerDateTime_DontUse().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeCalculatorShouldNotBeCalledAtAll();
			badgeWithRankCalculatorShouldNotBeCalledAtAll();
		}

		[Test]
		public void ShouldNotCalculateBadgeWhenNoBadgeTypeEnableForAppliedSetting()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);

			now.Stub(x => x.UtcDateTime()).Return(today);
			var newSetting = new GamificationSetting(settingNameForTest)
			{
				AdherenceBadgeEnabled = false,
				AHTBadgeEnabled = false,
				AnsweredCallsBadgeEnabled = false,
				GamificationSettingRuleSet = GamificationSettingRuleSet.RuleWithRatioConvertor
			};

			var team = TeamFactory.CreateSimpleTeam("team");
			team.SetId(Guid.NewGuid());

			var teamGamificationSetting = new TeamGamificationSetting
			{
				Team = team,
				GamificationSetting = newSetting
			};

			teamSettingsRepository.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam())
				.Return(new[] { teamGamificationSetting });

			var calculationDate = TimeZoneInfo.ConvertTime(now.ServerDateTime_DontUse().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = calculationDate
			};

			target.Calculate(message);

			badgeCalculatorShouldNotBeCalledAtAll();
			badgeWithRankCalculatorShouldNotBeCalledAtAll();
		}

		private void badgeCalculatorShouldNotBeCalledAtAll()
		{
			badgeCalculator.AssertWasNotCalled(x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId), o => o.IgnoreArguments());
			badgeCalculator.AssertWasNotCalled(x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, new GamificationSetting(settingNameForTest),
				_businessUnitId), o => o.IgnoreArguments());
			badgeCalculator.AssertWasNotCalled(x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId), o => o.IgnoreArguments());
		}

		private void badgeWithRankCalculatorShouldNotBeCalledAtAll()
		{
			badgeWithRankCalculator.AssertWasNotCalled(x => x.CalculateAHTBadges(new List<IPerson>(), "", DateOnly.Today,
				new GamificationSetting(settingNameForTest), _businessUnitId), o => o.IgnoreArguments());
			badgeWithRankCalculator.AssertWasNotCalled(x => x.CalculateAdherenceBadges(new List<IPerson>(), "", DateOnly.Today,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime, new GamificationSetting(settingNameForTest),
				_businessUnitId), o => o.IgnoreArguments());
			badgeWithRankCalculator.AssertWasNotCalled(x => x.CalculateAnsweredCallsBadges(new List<IPerson>(), "", DateOnly.Today,
					new GamificationSetting(settingNameForTest), _businessUnitId), o => o.IgnoreArguments());
		}
	}
}