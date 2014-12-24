﻿using System.Collections;
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
	public class AgentBadgeWithRankCalculatorTest
	{
		private const string timezoneCode = "";

		private IAgentBadgeWithRankCalculator _calculator;
		private DateOnly _calculateDateOnly;
		private Guid _lastPersonId;
		private List<IPerson> _allPersons;
		private IStatisticRepository _statisticRepository;
		private IAgentBadgeWithRankTransactionRepository _badgeTransactionRepository;
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
				BadgeEnabled = true,
				AnsweredCallsBadgeEnabled = true,
				AHTBadgeEnabled = true,
				AdherenceBadgeEnabled = true,
				CalculateBadgeWithRank = true,

				AnsweredCallsBronzeThreshold = 10,
				AnsweredCallsSilverThreshold = 20,
				AnsweredCallsGoldThreshold = 30,

				AHTBronzeThreshold = new TimeSpan(0, 15, 0),
				AHTSilverThreshold = new TimeSpan(0, 10, 0),
				AHTGoldThreshold = new TimeSpan(0, 5, 0),

				AdherenceBronzeThreshold = new Percent(0.6),
				AdherenceSilverThreshold = new Percent(0.75),
				AdherenceGoldThreshold = new Percent(0.9),
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

			_badgeTransactionRepository = MockRepository.GenerateMock<IAgentBadgeWithRankTransactionRepository>();
			_badgeTransactionRepository.Stub(x => x.Find(person, BadgeType.Adherence)).IgnoreArguments().Return(null);

			_now = MockRepository.GenerateMock<INow>();

			appFunctionFactory = MockRepository.GenerateMock<IDefinedRaptorApplicationFunctionFactory>();
			appFunctionFactory.Stub(x => x.ApplicationFunctionList).Return(new List<IApplicationFunction>
			{
				badgeFunction
			});

			_calculator = new AgentBadgeWithRankCalculator(_statisticRepository, _badgeTransactionRepository, appFunctionFactory,
				_now);
		}

		#region Adherence Badge Calculation

		[Test]
		public void ShouldNotAwardAdherenceBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 0.59}});

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAdherenceBronzeBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 0.61}});

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 1);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAdherenceSilverBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 0.75}});

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 1);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAdherenceGoldBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 0.91}});

			var result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		#endregion

		#region AHT Badge Calculation

		[Test]
		public void ShouldNotAwardAHTBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 1000}});
			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAHTBronzeBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 720}});
			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 1);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAHTSilverBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 420}});
			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 1);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAHTGoldBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 200}});
			var result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		#endregion

		#region Answered Call Badge Calculation

		[Test]
		public void ShoulNotdAwardAnsweredCallsBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 5}});
			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAnsweredCallsBronzeBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 15}});
			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 1);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAnsweredCallsSilverBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 25}});
			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 1);
			Assert.AreEqual(badge.GoldBadgeAmount, 0);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		[Test]
		public void ShouldAwardAnsweredCallsGoldBadgeForAgents()
		{
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 35}});
			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);

			var badge = result.Single(x => x.Person.Id == _lastPersonId);
			Assert.AreEqual(badge.Person.Id, _lastPersonId);
			Assert.AreEqual(badge.BronzeBadgeAmount, 0);
			Assert.AreEqual(badge.SilverBadgeAmount, 0);
			Assert.AreEqual(badge.GoldBadgeAmount, 1);
			Assert.AreEqual(badge.CalculatedDate, _calculateDateOnly);
		}

		#endregion

		[Test]
		public void ShouldNotAwardCalculateAnsweredCallsBadgeForAgentsWithoutPermission()
		{
			foreach (var person in _allPersons)
			{
				person.PermissionInformation.RemoveApplicationRole(_badgeRole);
			}

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 0.91}});

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 40}});

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new ArrayList {new object[] {_lastPersonId, DateTime.Now, 120}});

			var result = _calculator.CalculateAnsweredCallsBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);
			Assert.AreEqual(result.Any(), false);

			result = _calculator.CalculateAdherenceBadges(_allPersons, timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);
			Assert.AreEqual(result.Any(), false);

			result = _calculator.CalculateAHTBadges(_allPersons, timezoneCode, _calculateDateOnly,
				_badgeSetting);
			Assert.AreEqual(result.Any(), false);
		}
	}
}
