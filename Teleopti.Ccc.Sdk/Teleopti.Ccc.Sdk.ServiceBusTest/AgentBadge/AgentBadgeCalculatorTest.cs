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
	[TestFixture]
	public class AgentBadgeCalculatorTest
	{
		private const string _timezoneCode = "";

		private IAgentBadgeCalculator _calculator;
		private DateOnly _calculateDateOnly;
		private Guid _lastPersonId;
		private List<IPerson> _allPersons;
		private IStatisticRepository _statisticRepository;
		private AgentBadgeThresholdSettings _badgeSetting;

		[SetUp]
		public void Setup()
		{
			_badgeSetting = new AgentBadgeThresholdSettings()
			{
				AdherenceThreshold = new Percent(0.6),
				AHTThreshold = new TimeSpan(0, 5, 0),
				AnsweredCallsThreshold = 10,
				EnableBadge = true,
				GoldToSilverBadgeRate = 2,
				SilverToBronzeBadgeRate = 5
			};
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
						_timezoneCode, DateTime.Now, _badgeSetting.AdherenceThreshold))
				.IgnoreArguments().Return(new List<Guid> {_lastPersonId});

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsOverThresholdForAnsweredCalls(_timezoneCode, DateTime.Now, _badgeSetting.AnsweredCallsThreshold))
				.IgnoreArguments()
				.Return(new List<Guid> {_lastPersonId});

			_statisticRepository.Stub(
				x =>
					x.LoadAgentsUnderThresholdForAHT(_timezoneCode, DateTime.Now, _badgeSetting.AHTThreshold))
				.IgnoreArguments()
				.Return(new List<Guid> {_lastPersonId});

			_calculator = new AgentBadgeCalculator(_statisticRepository);
		}

		[Test]
		public void ShouldCalculateBadgeForCorrectAgents()
		{
			var result = _calculator.Calculate(_allPersons, _timezoneCode, _calculateDateOnly,
				AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime,
				_badgeSetting);

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
