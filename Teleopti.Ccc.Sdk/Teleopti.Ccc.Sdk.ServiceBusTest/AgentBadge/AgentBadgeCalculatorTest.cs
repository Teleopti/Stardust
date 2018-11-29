using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class AgentBadgeCalculatorTest
	{
		private const string timezoneCode = "";

		private IAgentBadgeCalculator _calculator;
		private DateOnly _calculateDateOnly;
		private Guid _lastPersonId;
		private IPerson[] _allPersons;
		private IBadgeCalculationRepository _badgeCalculationRepository;
		private IAgentBadgeTransactionRepository _badgeTransactionRepository;
		private IGamificationSetting _gamificationSetting;
		private INow _now;
		private IDefinedRaptorApplicationFunctionFactory appFunctionFactory;

		private IPersonRepository personRepository;
		private IScheduleStorage scheduleStorage;
		private IScenarioRepository scenarioRepository;

		private ApplicationRole _badgeRole;
		private Guid _businessUnitId;

		private IPerson lastPerson;
		private IScenario defaultScenario;
		private DateTime now;
		private FakeExternalPerformanceDataRepository _externalPerformanceDataRepository;

		[SetUp]
		public void Setup()
		{
			now = DateTime.Now;
			_calculateDateOnly = new DateOnly(now);

			var badgeFunctionCode = ApplicationFunction.GetCode(DefinedRaptorApplicationFunctionPaths.ViewBadge);
			var badgeFunction = new ApplicationFunction(badgeFunctionCode)
			{
				ForeignId = DefinedRaptorApplicationFunctionForeignIds.ViewBadge
			};
			_badgeRole = new ApplicationRole
			{
				Name = "Badge"
			};
			_badgeRole.AddApplicationFunction(badgeFunction);

			_gamificationSetting = new GamificationSetting("GamificationSetting4Test")
			{
				AdherenceThreshold = new Percent(0.6),
				AHTThreshold = new TimeSpan(0, 5, 0),
				AnsweredCallsThreshold = 10,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
			_allPersons = Enumerable.Range(0, 2).Select(i =>
			{
				var person = PersonFactory.CreatePersonWithPersonPeriod(_calculateDateOnly).WithId();
				person.PermissionInformation.AddApplicationRole(_badgeRole);
				return person;
			}).ToArray();
			
			lastPerson = _allPersons[1];
			_businessUnitId = new Guid();

			_lastPersonId = (Guid) lastPerson.Id;
			_badgeCalculationRepository = MockRepository.GenerateMock<IBadgeCalculationRepository>();
			_badgeCalculationRepository.Stub(
				x =>x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, now, _gamificationSetting.AdherenceThreshold, _businessUnitId))
				.IgnoreArguments().Return(new Dictionary<Guid, double> { { _lastPersonId, 0.01 } });

			_badgeCalculationRepository.Stub(
				x =>x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, now, _gamificationSetting.AnsweredCallsThreshold, _businessUnitId))
				.IgnoreArguments().Return(new Dictionary<Guid, int> { { _lastPersonId, 100}});

			_badgeCalculationRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAht(timezoneCode, now, _gamificationSetting.AHTThreshold, _businessUnitId))
				.IgnoreArguments()
				.Return(new Dictionary<Guid, double> { { _lastPersonId, 120}});

			_badgeTransactionRepository = MockRepository.GenerateMock<IAgentBadgeTransactionRepository>();
			_badgeTransactionRepository.Stub(x => x.Find(lastPerson, BadgeType.Adherence, DateOnly.Today, false)).IgnoreArguments().Return(null);

			_now = MockRepository.GenerateMock<INow>();

			appFunctionFactory = MockRepository.GenerateMock<IDefinedRaptorApplicationFunctionFactory>();
			appFunctionFactory.Stub(x => x.ApplicationFunctions).Return(new IApplicationFunction[]{ badgeFunction});

			// Stub for personRepository
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var personIdList = _allPersons.Select(x => x.Id.Value);
			personRepository.Stub(x => x.FindPeople(personIdList)).IgnoreArguments().Return(_allPersons);

			// Stub for scenarioRepository;
			scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			defaultScenario = new Scenario("default");
			scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(defaultScenario);

			// Stub for schedule
			scheduleStorage = MockRepository.GenerateMock<IScheduleStorage>();
			scheduleStorage.Stub(
				x =>
					x.FindSchedulesForPersonsOnlyInGivenPeriod(_allPersons, new ScheduleDictionaryLoadOptions(true, false),
						new DateOnlyPeriod(_calculateDateOnly, _calculateDateOnly), defaultScenario))
				.IgnoreArguments()
				.Return(new ScheduleDictionaryForTest(defaultScenario, now));
			_externalPerformanceDataRepository = new FakeExternalPerformanceDataRepository();
			_calculator = new AgentBadgeCalculator(_badgeCalculationRepository, _badgeTransactionRepository, appFunctionFactory,
				personRepository, scheduleStorage, scenarioRepository, _now, _externalPerformanceDataRepository);
		}

		[Test]
		public void ShouldCalculateAdherenceBadgeForCorrectAgents()
		{
			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_gamificationSetting,	_businessUnitId);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.Amount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
			Assert.AreEqual(badge.IsExternal, false);
		}

		[Test]
		public void ShouldCalculateAhtBadgeForCorrectAgents()
		{
			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_gamificationSetting, _businessUnitId);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.Amount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
			Assert.AreEqual(badge.IsExternal, false);
		}

		[Test]
		public void ShouldCalculateAnsweredCallsBadgeForCorrectAgents()
		{
			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_gamificationSetting, _businessUnitId);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.Amount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
			Assert.AreEqual(badge.IsExternal, false);
		}

		[Test]
		public void ShouldGetBadgeWhenScoreBiggerThanThreshold()
		{
			var badgeSetting = new BadgeSetting()
			{
				Threshold = 85,
				QualityId = 1
			};
			_gamificationSetting.AddBadgeSetting(badgeSetting);
			_externalPerformanceDataRepository.Add(new ExternalPerformanceData()
			{
				DateFrom = _calculateDateOnly,
				ExternalPerformance = new ExternalPerformance() { ExternalId = 1 },
				PersonId = _allPersons.First().Id.Value,
				Score = 91
			});

			var result = _calculator.CalculateBadges(_allPersons, _calculateDateOnly, badgeSetting, Guid.NewGuid());

			result.First().Amount.Should().Be.EqualTo(1);
			result.First().IsExternal.Should().Be.True();
		}

		[Test]
		public void ShouldNotGetBadgeWhenScoreSmallerThanThreshold()
		{
			var badgeSetting = new BadgeSetting()
			{
				Threshold = 85,
				QualityId = 1
			};
			_gamificationSetting.AddBadgeSetting(badgeSetting);
			_externalPerformanceDataRepository.Add(new ExternalPerformanceData()
			{
				DateFrom = _calculateDateOnly,
				ExternalPerformance = new ExternalPerformance() { ExternalId = 1 },
				PersonId = _allPersons.First().Id.Value,
				Score = 80
			});

			var result = _calculator.CalculateBadges(_allPersons, _calculateDateOnly, badgeSetting, Guid.NewGuid());

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotAwardAnsweredCallsBadgeForAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_gamificationSetting, _businessUnitId);

			Assert.AreEqual(result.Any(), false);
		}

		[Test]
		public void ShouldNotAwardAdherenceBadgeForAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_gamificationSetting, _businessUnitId);

			Assert.AreEqual(result.Any(), false);
		}

		[Test]
		public void ShouldNotAwardAhtBadgeForAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_gamificationSetting, _businessUnitId);

			Assert.AreEqual(result.Any(), false);
		}

		[Test]
		public void ShouldNotAwardAdherenceBadgeForAgentsOnFullDayAbsence()
		{
			// Create a schedule with full day abasence
			var utcNow = now.ToUniversalTime();
			var period = new DateTimePeriod(utcNow.Date.AddDays(-10), utcNow.Date.AddDays(10));
			var scheduleDict = new ScheduleDictionaryForTest(defaultScenario, now);

			var absence = new Absence { Description = new Description("TestAbsence") };
			var absenceLayer = new AbsenceLayer(absence, period);
			var personAbsence = new PersonAbsence(lastPerson, defaultScenario, absenceLayer);
			scheduleDict.AddPersonAbsence(personAbsence);

			scheduleStorage = MockRepository.GenerateMock<IScheduleStorage>();
			scheduleStorage.Stub(
				x =>
					x.FindSchedulesForPersonsOnlyInGivenPeriod(_allPersons, new ScheduleDictionaryLoadOptions(true, false),
						new DateOnlyPeriod(_calculateDateOnly, _calculateDateOnly), defaultScenario))
				.IgnoreArguments()
				.Return(scheduleDict);

			_calculator = new AgentBadgeCalculator(_badgeCalculationRepository, _badgeTransactionRepository, appFunctionFactory,
				personRepository, scheduleStorage, scenarioRepository, _now, _externalPerformanceDataRepository);

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_gamificationSetting, _businessUnitId);

			Assert.AreEqual(result.Any(), false);
		}
	}
}