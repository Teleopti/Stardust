using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping
{
	[TestFixture]
	public class WeekScheduleDomainDataMappingTest
	{
		private IScheduleProvider scheduleProvider;
		private IProjectionProvider projectionProvider;
		private IPersonRequestProvider personRequestProvider;
		private IUserTimeZone userTimeZone;

		[SetUp]
		public void Setup()
		{
			scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(
				new WeekScheduleDomainDataMappingProfile(
					Depend.On(scheduleProvider),
					Depend.On(projectionProvider),
					Depend.On(personRequestProvider),
					Depend.On(userTimeZone)
					)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var date = new DateOnly(DateTime.Now);
			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Date.Should().Be(date);
		}

		[Test]
		public void ShouldMapAWeeksDays()
		{
			var date = new DateOnly(DateTime.Now);
			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);
			result.Days.Should().Have.Count.EqualTo(7);
		}

		[Test]
		public void ShouldMapWeeksDates()
		{
			var date = new DateOnly(DateTime.Now);
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var datesInWeek = (from d in firstDayOfWeek.Date.DateRange(7) select new DateOnly(d)).ToList();

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
			var period = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] {scheduleDay});

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).ScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldMapProjection()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var scheduleDay = new StubFactory().ScheduleDayStub(date);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] {scheduleDay});
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).Projection.Should().Be.SameInstanceAs(projection);
		}

		[Test]
		public void ShouldMapPersonRequests()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var personRequest = new PersonRequest(new Person(),
			                                      new TextRequest(
			                                      	new DateTimePeriod(
			                                      		DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
			                                      	));

			personRequestProvider.Stub(x => x.RetrieveRequests(period)).Return(new[] {personRequest});

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).PersonRequests.Single().Should().Be.SameInstanceAs(personRequest);
		}

		[Test]
		public void ShouldMapPersonRequestsStartingAtMidnight()
		{
			var date = DateOnly.Today;
			var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture));
			var lastDayOfWeek = new DateOnly(DateHelper.GetLastDateInWeek(date, CultureInfo.CurrentCulture));
			var period = new DateOnlyPeriod(firstDayOfWeek, lastDayOfWeek);
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var localMidnightInUtc = timeZone.ConvertTimeToUtc(date.Date);

			var personRequest = new PersonRequest(new Person(),
			                                      new TextRequest(
			                                      	new DateTimePeriod(
			                                      		localMidnightInUtc, localMidnightInUtc.AddHours(1))
			                                      	));

			personRequestProvider.Stub(x => x.RetrieveRequests(period)).Return(new[] {personRequest});

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(date);

			result.Days.Single(d => d.Date == date).PersonRequests.Single().Should().Be.SameInstanceAs(personRequest);
		}

		[Test]
		public void ShouldMapColorSource()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			var projection = new StubFactory().ProjectionStub();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<DateOnly, WeekScheduleDomainData>(DateOnly.Today);

			result.ColorSource.ScheduleDays.Single().Should().Be(scheduleDay);
			result.ColorSource.Projections.Single().Should().Be.SameInstanceAs(projection);
		}
	}
}