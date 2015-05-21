﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class DefaultScenarioScheduleProviderTest
	{
		private DefaultScenarioScheduleProvider _target;
		private ICurrentScenario _scenarioProvider;
		private IScheduleRepository _scheduleRepository;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{
			_scenarioProvider = MockRepository.GenerateMock<ICurrentScenario>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			_target = new DefaultScenarioScheduleProvider(_loggedOnUser, _scheduleRepository, _scenarioProvider);
		}

		[Test]
		public void ShouldReturnOneScheduleDayForEachDateInPeriod()
		{
			var period = new DateOnlyPeriod(2011, 5, 18, 2011, 5, 19);
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var person = MockRepository.GenerateMock<IPerson>();
			var scenario = MockRepository.GenerateMock<IScenario>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			_scenarioProvider.Stub(x => x.Current()).Return(scenario);
			_scheduleRepository.Stub(x => x.FindSchedulesForPersonOnlyInGivenPeriod(
				person,
				new ScheduleDictionaryLoadOptions(true, true),
				period,
				scenario)).Return(scheduleDictionary).IgnoreArguments();
			scheduleDictionary.Stub(x => x[person]).Return(scheduleRange);
			scheduleRange.Stub(x => x.ScheduledDayCollection(period)).Return(new[] { scheduleDay, scheduleDay });

			var result = _target.GetScheduleForPeriod(period).ToList();
			result.Count().Should().Be.EqualTo(2);
			result.Any(r => r == null).Should().Be.False();
		}

		[Test]
		public void ShouldReturnOneScheduleDayForEachDateInPeriodForStudentAvailability()
		{
			var period = new DateOnlyPeriod(2011, 5, 18, 2011, 5, 19);
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var person = MockRepository.GenerateMock<IPerson>();
			var scenario = MockRepository.GenerateMock<IScenario>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			_scenarioProvider.Stub(x => x.Current()).Return(scenario);
			_scheduleRepository.Stub(x => x.FindSchedulesForPersonOnlyInGivenPeriod(
				person,
				new ScheduleDictionaryLoadOptions(true, true),
				period,
				scenario)).Return(scheduleDictionary).IgnoreArguments();
			scheduleDictionary.Stub(x => x[person]).Return(scheduleRange);
			scheduleRange.Stub(x => x.ScheduledDayCollectionForStudentAvailability(period)).Return(new[] { scheduleDay, scheduleDay });

			var result = _target.GetScheduleForStudentAvailability(period).ToList();
			result.Count().Should().Be.EqualTo(2);
			result.Any(r => r == null).Should().Be.False();
		}

		[Test]
		public void ShouldGetScheduleForPersons()
		{
			var user = new Person();
			user.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			var date = new DateOnly(2011, 5, 18);
			var period = new DateOnlyPeriod(date, date);
			var persons = new IPerson[] {user};
			var scenario = new Scenario("s");
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleDays = new IScheduleDay[] {};

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(user);
			_scenarioProvider.Stub(x => x.Current()).Return(scenario);
			_scheduleRepository.Stub(x => x.FindSchedulesForPersonsOnlyInGivenPeriod(
				Arg<IEnumerable<IPerson>>.Matches(o => o.Single() == user),
				Arg<IScheduleDictionaryLoadOptions>.Is.Anything,
				Arg<DateOnlyPeriod>.Is.Equal(period),
				Arg<IScenario>.Is.Equal(scenario)))
				.Return(scheduleDictionary);
			scheduleDictionary.Stub(x => x.SchedulesForDay(date)).Return(scheduleDays);

			var result = _target.GetScheduleForPersons(date, persons);

			result.Should().Be.SameInstanceAs(scheduleDays);
		}
	}
}
