﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class DefaultScenarioScheduleProviderTest
	{
		private DefaultScenarioScheduleProvider _target;
		private IScenarioProvider _scenarioProvider;
		private IScheduleRepository _scheduleRepository;
		private ILoggedOnUser _loggedOnUser;
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{
			_scenarioProvider = MockRepository.GenerateMock<IScenarioProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_target = new DefaultScenarioScheduleProvider(_loggedOnUser, _scheduleRepository, _scenarioProvider, _userTimeZone);
		}

		[Test]
		public void ShouldReturnOneScheduleDayForEachDateInPeriod()
		{
			var period = new DateOnlyPeriod(2011, 5, 18, 2011, 5, 19);
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = MockRepository.GenerateMock<IScheduleRange>();
			var timeZone = CccTimeZoneInfoFactory.StockholmTimeZoneInfo();
			var person = MockRepository.GenerateMock<IPerson>();
			var scenario = MockRepository.GenerateMock<IScenario>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			_scenarioProvider.Stub(x => x.DefaultScenario()).Return(scenario);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(true, true),
																			period.ToDateTimePeriod(timeZone),
																			scenario)).Return(scheduleDictionary).IgnoreArguments();
			scheduleDictionary.Stub(x => x[person]).Return(scheduleRange);
			scheduleRange.Stub(x => x.ScheduledDayCollection(period)).Return(new[] { scheduleDay, scheduleDay });

			var result = _target.GetScheduleForPeriod(period);
			result.Count().Should().Be.EqualTo(2);
			result.Any(r => r == null).Should().Be.False();
		}

		[Test]
		public void ShouldGetScheduleForPersons()
		{
			var user = new Person();
			user.PermissionInformation.SetDefaultTimeZone(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
			var date = new DateOnly(2011, 5, 18);
			var period = new DateOnlyPeriod(date, date);
			var dateTimePeriod = period.ToDateTimePeriod(CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
			var persons = new IPerson[] {user};
			var scenario = new Scenario(" ");
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleDays = new IScheduleDay[] {};

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(user);
			_scenarioProvider.Stub(x => x.DefaultScenario()).Return(scenario);
			_scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(
				Arg<IPersonProvider>.Matches(o => o.GetPersons().Single().Equals(user)),
				Arg<IScheduleDictionaryLoadOptions>.Is.Anything,
				Arg<DateTimePeriod>.Is.Equal(dateTimePeriod),
				Arg<IScenario>.Is.Equal(scenario)))
				.Return(scheduleDictionary);
			scheduleDictionary.Stub(x => x.SchedulesForDay(date)).Return(scheduleDays);

			var result = _target.GetScheduleForPersons(date, persons);

			result.Should().Be.SameInstanceAs(scheduleDays);
		}

		[Test]
		public void ShouldGetStudentAvailabilityForDate()
		{
			var scheduleDays = new[]
										{
											MockRepository.GenerateMock<IScheduleDay>(),
											MockRepository.GenerateMock<IScheduleDay>()
										};
			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[] {MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>()};
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[1].Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>(personRestrictions)));
			studentAvailabilityDay.Stub(x => x.RestrictionCollection).Return(new ReadOnlyCollection<IStudentAvailabilityRestriction>(new List<IStudentAvailabilityRestriction>(new[] { studentAvailabilityRestriction })));

			var result = _target.GetStudentAvailabilityForDate(scheduleDays, date);

			result.Should().Be.SameInstanceAs(studentAvailabilityRestriction);
		}

		[Test]
		public void ShouldReturnNullWhenNoStudentAvailabilityForDate()
		{
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var date = DateOnly.Today;

			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));

			var result = _target.GetStudentAvailabilityForDate(new[] { scheduleDay }, date);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldNotThrowWhen2ScheduleDaysAreFound()
		{
			var scheduleDays = new[]
										{
											MockRepository.GenerateMock<IScheduleDay>(),
											MockRepository.GenerateMock<IScheduleDay>()
										};
			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[] { MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>() };
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[1].Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>(personRestrictions)));
			scheduleDays[1].Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>(new IScheduleData[] { })));
			studentAvailabilityDay.Stub(x => x.RestrictionCollection).Return(new ReadOnlyCollection<IStudentAvailabilityRestriction>(new List<IStudentAvailabilityRestriction>(new[] { studentAvailabilityRestriction })));

			_target.GetStudentAvailabilityForDate(scheduleDays, date);
		}

		[Test]
		public void ShouldThrowWhenTotally2StudentAvailabilityRestrictionsFound()
		{	
			var scheduleDays = new[]
										{
											MockRepository.GenerateMock<IScheduleDay>(),
											MockRepository.GenerateMock<IScheduleDay>()
										};
			var date = DateOnly.Today;
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var personRestrictions = new[] { MockRepository.GenerateMock<IScheduleData>(), studentAvailabilityDay, MockRepository.GenerateMock<IScheduleData>() };
			var studentAvailabilityRestriction = MockRepository.GenerateMock<IStudentAvailabilityRestriction>();

			scheduleDays[0].Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[1].Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			scheduleDays[0].Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>(personRestrictions)));
			scheduleDays[1].Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>(personRestrictions)));
			studentAvailabilityDay.Stub(x => x.RestrictionCollection).Return(new ReadOnlyCollection<IStudentAvailabilityRestriction>(new List<IStudentAvailabilityRestriction>(new[] { studentAvailabilityRestriction })));

			Assert.Throws<MoreThanOneStudentAvailabilityFoundException>(() => _target.GetStudentAvailabilityForDate(scheduleDays, date));
		}

	}

	[TestFixture]
	public class PreferenceProviderTest
	{
		
		[Test]
		public void ShouldGetPreferencesForPeriod()
		{
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var target = new PreferenceProvider(scheduleProvider);
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();

			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(new ReadOnlyCollection<IScheduleData>(new[] {preferenceDay, MockRepository.GenerateMock<IScheduleData>()}));
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] {scheduleDay});

			var result = target.GetPreferencesForPeriod(period);

			result.Single().Should().Be.SameInstanceAs(preferenceDay);
		}
	}
}
