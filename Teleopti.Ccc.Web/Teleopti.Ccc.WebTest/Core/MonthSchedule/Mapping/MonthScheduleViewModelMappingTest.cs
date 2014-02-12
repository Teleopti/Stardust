using System;
using System.Drawing;
using System.Linq;
using AutoMapper;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.MonthSchedule.Mapping
{
	[TestFixture]
	public class MonthScheduleViewModelMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new MonthScheduleViewModelMappingProfile()));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }
        
		[Test]
		public void ShouldMapDate()
		{
		    var stub = new StubFactory();

            var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };
			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(domainData);

			result.Date.Should().Be.EqualTo(DateTime.Today);
		}

		[Test]
		public void ShouldMapFixedDate()
		{
            var stub = new StubFactory();


            var domainData = new MonthScheduleDayDomainData { ScheduleDay = stub.ScheduleDayStub(DateTime.Today) };
			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(domainData);

			result.FixedDate.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}


		[Test]
		public void ShouldMapIsWorkingDayForMainShift()
		{
			var stubs = new StubFactory();
			var personAssignment =
				stubs.PersonAssignmentStub(new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
				                                              new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc)));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);
		    scheduleDay.PersonAssignment().ShiftCategory.DisplayColor = Color.DarkGreen;


			var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData{ScheduleDay = scheduleDay});

			result.IsWorkingDay.Should().Be.True();			
            result.DisplayColor.Should().Be.EqualTo(scheduleDay.PersonAssignment().ShiftCategory.DisplayColor.ToHtml());
		}

        [Test]
        public void ShouldMapIsNotWorkingDayForDayOff()
        {
            var stubs = new StubFactory();
            var personAssignment =
                stubs.PersonAssignmentStub(new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
                                                              new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc)));
            var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.DayOff, personAssignment);

            var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

            result.IsNotWorkingDay.Should().Be.True();
        }

        [Test]
        public void ShouldMapIsNotWorkingDayForFullDayAbsence()
        {
            var stubs = new StubFactory();
            var personAbsence =
                stubs.PersonAbsenceStub(new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
                    new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc)));
            var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);

            var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDay });

            result.IsNotWorkingDay.Should().Be.True();
        }

        [Test]
        public void ShouldMapIsNotWorkingDayForFullDayAbsenceOnDayOff()
        {
            var stubs = new StubFactory();
            var personAssignment =
                stubs.PersonAssignmentStub(new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
                                                              new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc)));
            personAssignment.Clear();
            personAssignment.SetDayOff(DayOffFactory.CreateDayOff());

            var personAbsence =
                stubs.PersonAbsenceStub(new DateTimePeriod(new DateTime(2011, 5, 18, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2011, 5, 18, 23, 59, 0, DateTimeKind.Utc)));
            var scheduleDayForDayoff = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.ContractDayOff, personAssignment, new[] {personAbsence}, null);

            var result = Mapper.Map<MonthScheduleDayDomainData, MonthDayViewModel>(new MonthScheduleDayDomainData { ScheduleDay = scheduleDayForDayoff });

            result.IsNotWorkingDay.Should().Be.True();
        }

        [Test]
        public void ShouldMapDaysOfMonth()
        {
            var stub = new StubFactory();

            var domainData = new MonthScheduleDayDomainData {ScheduleDay = stub.ScheduleDayStub(DateTime.Today)};
            var monthDomainData = new MonthScheduleDomainData {Days = new[]{domainData}};
            var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);

            result.ScheduleDays.Count().Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldMapCurrentDate()
        {
            var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] {},CurrentDate = DateOnly.Today};
            var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);
            result.CurrentDate.Should().Be.EqualTo(DateTime.Today);
        }

        [Test]
        public void ShouldMapFixedCurrentDate()
        {
            var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };
            var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);
            result.FixedDate.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
        }

	    [Test]
        [SetCulture("de-DE")]
	    public void ShouldMapDayHeaderOfWeek()
	    {
            var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { }, CurrentDate = DateOnly.Today };
            var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);
            result.DayHeaders.First().Name.Should().Be.EqualTo("Montag");
            result.DayHeaders.First().ShortName.Should().Be.EqualTo("Mo");
        }
        
        [Test]
        public void ShouldMapAbsenceName()
        {
            var stubs = new StubFactory();
            var personAbsence =
                new PersonAbsence(new Person(), new Scenario(" "), new AbsenceLayer(new Absence(){Description = new Description("Illness")}, new DateTimePeriod()) );
            var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.FullDayAbsence, personAbsence);

            var monthDomainData = new MonthScheduleDomainData { Days = new MonthScheduleDayDomainData[] { new MonthScheduleDayDomainData() { ScheduleDay=scheduleDay }}, CurrentDate = DateOnly.Today };
            var result = Mapper.Map<MonthScheduleDomainData, MonthScheduleViewModel>(monthDomainData);
            result.ScheduleDays.Single().Absence.Should().Be.EqualTo("Illness");
        }
	}
}