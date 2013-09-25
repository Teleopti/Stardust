﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;
using IAllowanceProvider = Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider.IAllowanceProvider;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping
{
	[TestFixture]
	public class WeekScheduleDomainDataMappingTest
	{
		private IScheduleProvider scheduleProvider;
		private IProjectionProvider projectionProvider;
		private IPersonRequestProvider personRequestProvider;
		private IUserTimeZone userTimeZone;
		private IPrincipal principalBefore;
		private TimeZoneInfo timeZone;
		private IPermissionProvider permissionProvider;
		private INow now;
		private IAllowanceProvider allowanceProvider;
		private IAbsenceTimeProvider absenceTimeProvider;

		[SetUp]
		public void Setup()
		{
			timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();
			setPrincipal();

			scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			now = MockRepository.GenerateMock<INow>();
			allowanceProvider = MockRepository.GenerateMock<IAllowanceProvider>();
			absenceTimeProvider = MockRepository.GenerateMock<IAbsenceTimeProvider>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(
				new WeekScheduleDomainDataMappingProfile(
					scheduleProvider,
					projectionProvider,
					personRequestProvider,
					userTimeZone,
					permissionProvider,
					now,
					allowanceProvider,
					absenceTimeProvider
					)));
		}

		private void setPrincipal()
		{
			principalBefore = Thread.CurrentPrincipal;
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(
					 new TeleoptiIdentity("test", null, null, null), person);
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldMapAWeeksDays()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Days.Should().Have.Count.EqualTo(7);
		}

		[Test]
		public void ShouldMapWeeksDates()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();
			var datesInWeek = (from d in firstDayOfWeek.Date.DateRange(7) select new DateOnly(d)).ToList();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Select(x => x.Date)
				.Should().Have.SameSequenceAs(datesInWeek);
		}

		[Test]
		public void ShouldMapScheduleDay()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldMapProjection()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).Projection.Should().Be.SameInstanceAs(projection);
		}
       
        [Test]
        public void ShouldMapProjectionIncludingTheDayBeforeCurrentWeek()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
            var date = new DateOnly(2012, 08, 27);
            var yesterdayDate = new DateOnly(2012, 08, 26);
            var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
            var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), firstDayOfWeek.AddDays(6));
            var scheduleDay = new StubFactory().ScheduleDayStub(date);
            var scheduleYesterday = new StubFactory().ScheduleDayStub(yesterdayDate);
            var projectionYesterday = new StubFactory().ProjectionStub();

            scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay, scheduleYesterday });
            projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projectionYesterday);

            var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

            result.Days.Single(d => d.Date == date).ProjectionYesterday.Should().Not.Be.Null();
            result.Days.Single(d => d.Date == date).ProjectionYesterday.Should().Be.SameInstanceAs(projectionYesterday);
        }

		[Test]
		public void ShouldMapOvertimeAvailability()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var overtimeAvailability = new OvertimeAvailability(new Person(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0));
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
							 .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability> { overtimeAvailability }));
			var projection = new StubFactory().ProjectionStub();
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).OvertimeAvailability.Should().Be.SameInstanceAs(overtimeAvailability);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityForYesterday()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
			var date = new DateOnly(2012, 08, 27);
			var yesterdayDate = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), firstDayOfWeek.AddDays(6));
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var scheduleYesterday = new StubFactory().ScheduleDayStub(yesterdayDate);
			var overtimeAvailabilityYesterday = new OvertimeAvailability(new Person(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0));
			scheduleYesterday.Stub(x => x.OvertimeAvailablityCollection())
			                 .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability> {overtimeAvailabilityYesterday}));
			var projectionYesterday = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay, scheduleYesterday });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projectionYesterday);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).OvertimeAvailabilityYesterday.Should().Not.Be.Null();
			result.Days.Single(d => d.Date == date).OvertimeAvailabilityYesterday.Should().Be.SameInstanceAs(overtimeAvailabilityYesterday);
		}

		[Test]
		public void ShouldMapPersonRequests()
		{
			var date = new DateOnly(DateTime.UtcNow.Date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

            scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var personRequest = new PersonRequest(new Person(),
			                                      new TextRequest(
			                                      	new DateTimePeriod(
			                                      		DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
			                                      	));

			personRequestProvider.Stub(x => x.RetrieveRequests(week)).Return(new[] {personRequest});

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).PersonRequests.Single().Should().Be.SameInstanceAs(personRequest);
		}

		[Test]
		public void ShouldMapAllowances()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var allowance = TimeSpan.FromHours(4);
			var fulltimeEquivalents = TimeSpan.FromHours(0);
			var allowanceDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate, allowance, fulltimeEquivalents, true);
			var allowanceDay2 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(1), allowance, fulltimeEquivalents, true);
			var allowanceDay3 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(2), allowance, fulltimeEquivalents, true);
			var allowanceDay4 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(3), allowance, fulltimeEquivalents, true);
			var allowanceDay5 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(4), allowance, fulltimeEquivalents, true);
			var allowanceDay6 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(5), allowance, fulltimeEquivalents, true);
			var allowanceDay7 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(6), allowance.Add(TimeSpan.FromHours(1)), fulltimeEquivalents, true);
 
			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(week)).Return(new[] { allowanceDay1, allowanceDay2, allowanceDay3, allowanceDay4, allowanceDay5, allowanceDay6, allowanceDay7 });

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Days.Single(d => d.Date == lastDayOfWeek).Allowance.Should().Be.EqualTo(allowanceDay7.Item2.TotalMinutes);
		}

		[Test]
		public void ShouldMapAvailabilities()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var allowance = TimeSpan.FromHours(4);
			const bool availability = false;
			var fulltimeEquivalents = TimeSpan.FromHours(0);

			var availabilityDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate, allowance, fulltimeEquivalents, availability);
			var availabilityDay2 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(1), allowance, fulltimeEquivalents, availability);
			var availabilityDay3 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(2), allowance, fulltimeEquivalents, availability);
			var availabilityDay4 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(3), allowance, fulltimeEquivalents, availability);
			var availabilityDay5 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(4), allowance, fulltimeEquivalents, availability);
			var availabilityDay6 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(5), allowance, fulltimeEquivalents, availability);
			var availabilityDay7 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(6), allowance.Add(TimeSpan.FromHours(1)), fulltimeEquivalents, availability);
			
			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(week)).Return(new[] { availabilityDay1, availabilityDay2, availabilityDay3, availabilityDay4, availabilityDay5, availabilityDay6, availabilityDay7 });

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Days.Single(d => d.Date == lastDayOfWeek).Availability.Should().Be.EqualTo(availabilityDay7.Item4);
		}

		[Test]
		public void ShouldMapFulltimeEquivalents()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var allowance = TimeSpan.FromHours(4);
			const bool availability = false;
			var fulltimeEquivalents = TimeSpan.FromHours(4);

			var availabilityDay1 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate, allowance, fulltimeEquivalents, availability);
			var availabilityDay2 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(1), allowance, fulltimeEquivalents, availability);
			var availabilityDay3 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(2), allowance, fulltimeEquivalents, availability);
			var availabilityDay4 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(3), allowance, fulltimeEquivalents, availability);
			var availabilityDay5 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(4), allowance, fulltimeEquivalents, availability);
			var availabilityDay6 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(5), allowance, fulltimeEquivalents, availability);
			var availabilityDay7 = new Tuple<DateOnly, TimeSpan, TimeSpan, bool>(week.StartDate.AddDays(6), allowance.Add(TimeSpan.FromHours(1)), fulltimeEquivalents, availability);

			allowanceProvider.Stub(x => x.GetAllowanceForPeriod(week)).Return(new[] { availabilityDay1, availabilityDay2, availabilityDay3, availabilityDay4, availabilityDay5, availabilityDay6, availabilityDay7 });

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Days.Single(d => d.Date == lastDayOfWeek).FulltimeEquivalent.Should().Be.EqualTo(availabilityDay7.Item3.TotalMinutes);
		}

		[Test]
		public void ShouldMapPersonRequestsStartingAtMidnight()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
            var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
            var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
            var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);

			var personRequest = new PersonRequest(new Person(),
			                                      new TextRequest(
			                                      	new DateTimePeriod(
			                                      		localMidnightInUtc, localMidnightInUtc.AddHours(1))
			                                      	));

			personRequestProvider.Stub(x => x.RetrieveRequests(week)).Return(new[] {personRequest});

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).PersonRequests.Single().Should().Be.SameInstanceAs(personRequest);
		}

		[Test]
		public void ShouldMapColorSource()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(DateOnly.Today);

			result.ColorSource.ScheduleDays.Single().Should().Be(scheduleDay);
			result.ColorSource.Projections.Single().Should().Be.SameInstanceAs(projection);
		}

		[Test]
		public void ShouldMapMinMaxTime()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(8), localMidnightInUtc.AddHours(17));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(7);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(17);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

        [Test]
        public void ShouldMapMinMaxTimeForNightShift()
        {
            var date = new DateOnly(2012,08,28);
            var scheduleDay = new StubFactory().ScheduleDayStub(date);

            userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
            var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);
            var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

						var layer = new StubFactory().VisualLayerStub(projectionPeriod);
            var projection = new StubFactory().ProjectionStub(new[] { layer });

            scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
            projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

            var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

            result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
            result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
        }

        [Test]
        public void ShouldMapMinMaxTimeForNightShiftFromPreviousWeek()
        {
            var date = new DateOnly(2012, 08, 28);
            var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
            var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
            var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

            var scheduleDay = new StubFactory().ScheduleDayStub(firstDayOfWeek.AddDays(-1).Date);   

            userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
            var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(firstDayOfWeek.AddDays(-1).Date);
            var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

						var layer = new StubFactory().VisualLayerStub(projectionPeriod);
            var projection = new StubFactory().ProjectionStub(new[] { layer });

            scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
            projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

            var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(firstDayOfWeek);

            result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
            result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

            result.MinMaxTime.EndTime.Days.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
        }

        [Test]
        public void ShouldMapMinMaxTimeForNightShiftStartingOnTheLastDayOfCurrentWeek()
        {
            var date = new DateOnly(2012, 08, 26);
            var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
            var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
            var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

            var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

            userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
            var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
            var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

						var layer = new StubFactory().VisualLayerStub(projectionPeriod);
            var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
            projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

            var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(lastDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
        }

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailability()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
			           .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>
				           {
					           new OvertimeAvailability(new Person(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0))
				           }));

			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.Period()).Return(null);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(2);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShift()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
					   .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>
						   {
					           new OvertimeAvailability(new Person(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0))
				           }));

			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.Period()).Return(null);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(0);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftFromPreviousWeek()
		{
			var date = new DateOnly(2012, 08, 28);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(firstDayOfWeek.AddDays(-1).Date);
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
					   .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>
						   {
					           new OvertimeAvailability(new Person(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0))
				           }));

			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.Period()).Return(null);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(firstDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

			result.MinMaxTime.EndTime.Days.Should().Be.EqualTo(0);
			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(4);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftStartingOnTheLastDayOfCurrentWeek()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
					   .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>
						   {
					           new OvertimeAvailability(new Person(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0))
				           }));

			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.Period()).Return(null);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(lastDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapAsmPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });
			
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(firstDayOfWeek);

			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapRequestPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).Return(true);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(firstDayOfWeek);

			result.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsCurrentWeek()
		{
			var date = new DateTime(2014, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			now.Stub(x => x.UtcDateTime()).Return(date);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(firstDayOfWeek);

			result.IsCurrentWeek.Should().Be.True();
		}

		
		[TearDown]
		public void Teardown()
		{
			System.Threading.Thread.CurrentPrincipal = principalBefore;
		}
	}
}