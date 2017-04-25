using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class WeekScheduleDomainDataProviderTest
	{
		private IScheduleProvider scheduleProvider;
		private IProjectionProvider projectionProvider;
		private IPersonRequestProvider personRequestProvider;
		private TimeZoneInfo timeZone;
		private IPermissionProvider permissionProvider;
		private INow now;
		private IAbsenceRequestProbabilityProvider probabilityProvider;
		private ISeatOccupancyProvider seatBookingProvider;
		private IWeekScheduleDomainDataProvider target;

		[SetUp]
		public void Setup()
		{
			timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			now = MockRepository.GenerateMock<INow>();
			probabilityProvider = MockRepository.GenerateMock<IAbsenceRequestProbabilityProvider>();
			seatBookingProvider = MockRepository.GenerateMock<ISeatOccupancyProvider>();

			target = new WeekScheduleDomainDataProvider(scheduleProvider,
					projectionProvider,
					personRequestProvider,
					seatBookingProvider,
					new FuncTimeZone(() => timeZone),
					permissionProvider,
					now,
					probabilityProvider,
					new FakeUserCulture(CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldMapDate()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);
			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldMapAWeeksDays()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);
			result.Days.Should().Have.Count.EqualTo(7);
		}

		[Test]
		public void ShouldMapWeeksDates()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();
			var datesInWeek = (from d in firstDayOfWeek.Date.DateRange(7) select new DateOnly(d)).ToList();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.Days.Select(x => x.Date)
				.Should().Have.SameSequenceAs(datesInWeek);
		}

		[Test]
		public void ShouldMapScheduleDay()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldMapProjection()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).Projection.Should().Be.SameInstanceAs(projection);
		}

		[Test]
		public void ShouldMapProjectionIncludingTheDayBeforeCurrentWeek()
		{
			var date = new DateOnly(2012, 08, 27);
			var yesterdayDate = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), firstDayOfWeek.AddDays(6));
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var scheduleYesterday = new StubFactory().ScheduleDayStub(yesterdayDate.Date);
			var projectionYesterday = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay, scheduleYesterday });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projectionYesterday);

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).ProjectionYesterday.Should().Not.Be.Null();
			result.Days.Single(d => d.Date == date).ProjectionYesterday.Should().Be.SameInstanceAs(projectionYesterday);
		}

		[Test]
		public void ShouldMapOvertimeAvailability()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var overtimeAvailability = new OvertimeAvailability(new Person(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0));
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
							 .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability> { overtimeAvailability }));
			var projection = new StubFactory().ProjectionStub();
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).OvertimeAvailability.Should().Be.SameInstanceAs(overtimeAvailability);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityForYesterday()
		{
			var date = new DateOnly(2012, 08, 27);
			var yesterdayDate = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), firstDayOfWeek.AddDays(6));
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var scheduleYesterday = new StubFactory().ScheduleDayStub(yesterdayDate.Date);
			var overtimeAvailabilityYesterday = new OvertimeAvailability(new Person(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0));
			scheduleYesterday.Stub(x => x.OvertimeAvailablityCollection())
							 .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability> { overtimeAvailabilityYesterday }));
			var projectionYesterday = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay, scheduleYesterday });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projectionYesterday);

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).OvertimeAvailabilityYesterday.Should().Not.Be.Null();
			result.Days.Single(d => d.Date == date).OvertimeAvailabilityYesterday.Should().Be.SameInstanceAs(overtimeAvailabilityYesterday);
		}

		[Test]
		public void ShouldMapPersonRequests()
		{
			var date = new DateOnly(DateTime.UtcNow.Date);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var personRequest = new PersonRequest(new Person(),
												  new TextRequest(
													new DateTimePeriod(
														DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
													));

			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(week)).Return(new[] { personRequest });

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).PersonRequests.Single().Should().Be.SameInstanceAs(personRequest);
		}

		[Test]
		public void ShouldMapClass()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			const string theClass = "red";
			const bool availability = false;
			const string text = "poor";

			var probDay1 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate, Text = text };
			var probDay2 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(1), Text = text };
			var probDay3 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(2), Text = text };
			var probDay4 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(3), Text = text };
			var probDay5 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(4), Text = text };
			var probDay6 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(5), Text = text };
			var probDay7 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(6), Text = text };

			probabilityProvider
				.Stub(x => x.GetAbsenceRequestProbabilityForPeriod(week))
				.Return(new List<IAbsenceRequestProbability> { probDay1, probDay2, probDay3, probDay4, probDay5, probDay6, probDay7 });

			var result = target.GetWeekSchedule(date);
			result.Days.Single(d => d.Date == lastDayOfWeek).ProbabilityClass.Should().Be.EqualTo(probDay7.CssClass);
		}

		[Test]
		public void ShouldMapText()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			const string theClass = "red";
			const bool availability = false;
			const string text = "poor";

			var probDay1 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate, Text = text };
			var probDay2 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(1), Text = text };
			var probDay3 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(2), Text = text };
			var probDay4 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(3), Text = text };
			var probDay5 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(4), Text = text };
			var probDay6 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(5), Text = text };
			var probDay7 = new AbsenceRequestProbability { Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(6), Text = text };

			probabilityProvider
				.Stub(x => x.GetAbsenceRequestProbabilityForPeriod(week))
				.Return(new List<IAbsenceRequestProbability> { probDay1, probDay2, probDay3, probDay4, probDay5, probDay6, probDay7 });

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == lastDayOfWeek).ProbabilityText.Should().Be.EqualTo(probDay7.Text);
		}

		[Test]
		public void ShouldMapAvailability()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			const string theClass = "red";
			const bool availability = true;
			const string text = "poor";

			var probDay1 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate, Text = text} ;
			var probDay2 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(1), Text = text};
			var probDay3 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(2), Text = text};
			var probDay4 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(3), Text = text};
			var probDay5 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(4), Text = text};
			var probDay6 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(5), Text = text};
			var probDay7 = new AbsenceRequestProbability {Availability = availability, CssClass = theClass, Date = week.StartDate.AddDays(6), Text = text};

			probabilityProvider
				.Stub(x => x.GetAbsenceRequestProbabilityForPeriod(week))
				.Return(new List<IAbsenceRequestProbability> { probDay1, probDay2, probDay3, probDay4, probDay5, probDay6, probDay7 });

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == lastDayOfWeek).Availability.Should().Be.EqualTo(probDay7.Availability);
		}

		[Test]
		public void ShouldMapPersonRequestsStartingAtMidnight()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var week = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var weekWithPreviousDay = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(weekWithPreviousDay)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);

			var personRequest = new PersonRequest(new Person(),
												  new TextRequest(
													new DateTimePeriod(
														localMidnightInUtc, localMidnightInUtc.AddHours(1))
													));

			personRequestProvider.Stub(x => x.RetrieveRequestsForLoggedOnUser(week)).Return(new[] { personRequest });

			var result = target.GetWeekSchedule(date);

			result.Days.Single(d => d.Date == date).PersonRequests.Single().Should().Be.SameInstanceAs(personRequest);
		}

		[Test]
		public void ShouldMapColorSource()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.ColorSource.ScheduleDays.Single().Should().Be(scheduleDay);
			result.ColorSource.Projections.Single().Should().Be.SameInstanceAs(projection);
		}

		[Test]
		public void ShouldMapMinMaxTime()
		{
			var date = DateOnly.Today;
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(8), localMidnightInUtc.AddHours(17));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(7);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(17);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShift()
		{
			var date = new DateOnly(2012, 08, 28);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(00);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldHaveCorrectStartTimeForEndingAtMidNightEdgeCase()
		{
			var date = new DateOnly(2012, 08, 28);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(date.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(24));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForNightShiftFromPreviousWeek()
		{
			var date = new DateOnly(2012, 08, 28);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(firstDayOfWeek.AddDays(-1).Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(firstDayOfWeek.AddDays(-1).Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(firstDayOfWeek);

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
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(lastDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailability()
		{
			var date = new DateOnly(2013, 09, 11);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
					   .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>
						   {
							   new OvertimeAvailability(new Person(), date, new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0))
						   }));

			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.Period()).Return(null);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(2);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShift()
		{
			var date = new DateOnly(2013, 09, 11);
			var scheduleDay = new StubFactory().ScheduleDayStub(date.Date);
			scheduleDay.Stub(x => x.OvertimeAvailablityCollection())
					   .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>
						   {
							   new OvertimeAvailability(new Person(), date, new TimeSpan(20, 0, 0), new TimeSpan(28, 0, 0))
						   }));

			var projection = MockRepository.GenerateMock<IVisualLayerCollection>();
			projection.Stub(x => x.Period()).Return(null);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything, Arg<IScheduleDictionaryLoadOptions>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);

			var result = target.GetWeekSchedule(date);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(0);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(0);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapMinMaxTimeForOvertimeAvailabilityForNightShiftFromPreviousWeek()
		{
			var date = new DateOnly(2012, 08, 28);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
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

			var result = target.GetWeekSchedule(firstDayOfWeek);

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
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
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

			var result = target.GetWeekSchedule(lastDayOfWeek);

			result.MinMaxTime.StartTime.Hours.Should().Be.EqualTo(19);
			result.MinMaxTime.StartTime.Minutes.Should().Be.EqualTo(45);

			result.MinMaxTime.EndTime.Hours.Should().Be.EqualTo(23);
			result.MinMaxTime.EndTime.Minutes.Should().Be.EqualTo(59);
		}

		[Test]
		public void ShouldMapAsmPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);

			var result = target.GetWeekSchedule(firstDayOfWeek);

			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapViewPossibilityPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewStaffingInfo)).Return(true);

			var result = target.GetWeekSchedule(firstDayOfWeek);

			result.ViewPossibilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapRequestPermission()
		{
			var date = new DateOnly(2012, 08, 26);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek.AddDays(-1), lastDayOfWeek);

			var scheduleDay = new StubFactory().ScheduleDayStub(lastDayOfWeek.Date);

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).Return(true);

			var result = target.GetWeekSchedule(firstDayOfWeek);

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

			var localMidnightInUtc = timeZone.SafeConvertTimeToUtc(lastDayOfWeek.Date);
			var projectionPeriod = new DateTimePeriod(localMidnightInUtc.AddHours(20), localMidnightInUtc.AddHours(28));

			var layer = new StubFactory().VisualLayerStub(projectionPeriod);
			var projection = new StubFactory().ProjectionStub(new[] { layer });

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(Arg<IScheduleDay>.Is.Anything)).Return(projection);
			now.Stub(x => x.UtcDateTime()).Return(date);

			var result = target.GetWeekSchedule(firstDayOfWeek);

			result.IsCurrentWeek.Should().Be.True();
		}
	}
}