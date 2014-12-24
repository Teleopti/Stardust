using System.Collections;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class AgentBadgeCalculatorTest
	{
		private const string timezoneCode = "";

		private IAgentBadgeCalculator _calculator;
		private DateOnly _calculateDateOnly;
		private Guid _lastPersonId;
		private List<IPerson> _allPersons;
		private IStatisticRepository _statisticRepository;
		private IAgentBadgeTransactionRepository _badgeTransactionRepository;
		private AgentBadgeSettings _badgeSetting;
		private INow _now;
		private IDefinedRaptorApplicationFunctionFactory appFunctionFactory;

		private ApplicationRole _badgeRole;

		[SetUp]
		public void Setup()
		{
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

			_badgeSetting = new AgentBadgeSettings
			{
				AdherenceThreshold = new Percent(0.6),
				AHTThreshold = new TimeSpan(0, 5, 0),
				AnsweredCallsThreshold = 10,
				BadgeEnabled = true,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
			_allPersons = new List<IPerson>();

			IPerson person = null;
			for (var i = 0; i < 2; i++)
			{
				person = new Person();
				person.SetId(Guid.NewGuid());
				person.PermissionInformation.AddApplicationRole(_badgeRole);
				_allPersons.Add(person);
			}

			_calculateDateOnly = new DateOnly(2014, 08, 11);

			_lastPersonId = (Guid) person.Id;
			_statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new ArrayList { new object[] { _lastPersonId, DateTime.Now, 0.01 } });

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new ArrayList { new object[] { _lastPersonId, DateTime.Now, 100 } });

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new ArrayList { new object[] { _lastPersonId, DateTime.Now, 120 } });

			_badgeTransactionRepository = MockRepository.GenerateMock<IAgentBadgeTransactionRepository>();
			_badgeTransactionRepository.Stub(x => x.Find(person, BadgeType.Adherence)).IgnoreArguments().Return(null);

			_now = MockRepository.GenerateMock<INow>();

			appFunctionFactory = MockRepository.GenerateMock<IDefinedRaptorApplicationFunctionFactory>();
			appFunctionFactory.Stub(x => x.ApplicationFunctionList).Return(new List<IApplicationFunction>
			{
				badgeFunction
			});

			_calculator = new AgentBadgeCalculator(_statisticRepository, _badgeTransactionRepository, appFunctionFactory, _now);
		}

		[Test]
		public void ShouldCalculateAdherenceBadgeForCorrectAgents()
		{
			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.Amount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldCalculateAHTBadgeForCorrectAgents()
		{
			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.Amount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldCalculateAnsweredCallsBadgeForCorrectAgents()
		{
			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.Amount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldNotAwardCalculateAnsweredCallsBadgeForAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			Assert.AreEqual(result.Any(), false);
		}
		[Test]
		public void ShouldNotAwardAdherenceBadgeForCorrectAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

			Assert.AreEqual(result.Any(), false);
		}

		[Test]
		public void ShouldNotAwardAHTBadgeForCorrectAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			Assert.AreEqual(result.Any(), false);
		}
	}
}
