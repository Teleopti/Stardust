using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	class AgentBadgeCalculatorTest
	{
		private const string _timezoneCode = "";
		private const int silverToBronzeBadgeRate = 5;
		private const int goldToSilverBadgeRate = 2;

		private IAgentBadgeCalculator _calculator;
		private DateOnly _calculateDateOnly;
		private Guid _lastPersonId;
		private List<IPerson> _allPersons;
		private IStatisticRepository _statisticRepository;

		[SetUp]
		public void Setup()
		{
			_allPersons = new List<IPerson>();

			IPerson person = null;
			for (var i = 0; i < 2; i++)
			{
				person = new Person();
				person.SetId(Guid.NewGuid());
				_allPersons.Add(person);
			}

			_calculateDateOnly = new DateOnly(2014, 08, 11);

			_lastPersonId = (Guid) person.Id;
			_statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
						_timezoneCode, DateTime.Now))
				.IgnoreArguments().Return(new List<Guid> {_lastPersonId});

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(_timezoneCode, DateTime.Now))
				.IgnoreArguments()
				.Return(new List<Guid> {_lastPersonId});

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(_timezoneCode, DateTime.Now))
				.IgnoreArguments()
				.Return(new List<Guid> {_lastPersonId});

			_calculator = new AgentBadgeCalculator(_statisticRepository);
		}

		[Test]
		public void ShouldCalculateBadgeForCorrectAgents()
		{
			var result = _calculator.Calculate(_allPersons, _timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				silverToBronzeBadgeRate, goldToSilverBadgeRate);

			var lastPerson = result.First(x => x.Id == _lastPersonId);

			Assert.IsNotNull(lastPerson);

			var badge = lastPerson.Badges.Single(x => x.BadgeType == BadgeType.Adherence);
			Assert.AreEqual(badge.BronzeBadge, 1);
			Assert.AreEqual(badge.LastCalculatedDate, _calculateDateOnly);

			badge = lastPerson.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls);
			Assert.AreEqual(badge.BronzeBadge, 1);
			Assert.AreEqual(badge.LastCalculatedDate, _calculateDateOnly);

			badge = lastPerson.Badges.Single(x => x.BadgeType == BadgeType.AverageHandlingTime);
			Assert.AreEqual(badge.BronzeBadge, 1);
			Assert.AreEqual(badge.LastCalculatedDate, _calculateDateOnly);
		}
	}
}
