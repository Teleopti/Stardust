using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
    [TestFixture, SetCulture("sv-SE"), SetUICulture("sv-SE")]
    public class PeriodViewModelFactoryTest
    {
        private IPeriodViewModelFactory target;
        private MockRepository mocks;
        private TimeZoneInfo timeZone;
        private IVisualLayerCollection visualLayerCollection;
        private IPrincipal principalBefore;
        private DateTimePeriod period;
        private IVisualLayerFactory factory;
        private IActivity activity;
        private IVisualLayer visualActivityLayer;
        private TimePeriod minMaxTime;
        private IPerson person;
        private DateTime localDate;
	    private IMappingEngine _mapper;

	    [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();

            timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();

            setPrincipal();

            visualLayerCollection = mocks.DynamicMock<IVisualLayerCollection>();

            localDate = new DateTime(2011, 5, 18);
            period = new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc));
            factory = new VisualLayerFactory();
            activity = ActivityFactory.CreateActivity("Phone");
            activity.SetId(Guid.NewGuid());

		    var scheduleDay = ScheduleDayFactory.Create(new DateOnly(localDate));
			scheduleDay.CreateAndAddActivity(activity, period,new ShiftCategory("Shift Category") );

            minMaxTime = new TimePeriod(8, 0, 19, 0);

            visualActivityLayer = factory.CreateShiftSetupLayer(activity, period, person);

			_mapper = MockRepository.GenerateMock<IMappingEngine>();
			target = new PeriodViewModelFactory(_mapper, new SpecificTimeZone(timeZone));
        }

        private void setPrincipal()
        {
            principalBefore = System.Threading.Thread.CurrentPrincipal;
            person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone(timeZone);
            System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
                     new TeleoptiIdentity("test", null, null, null, null), person);
        }

        [Test]
        public void ShouldCreatePeriodViewModelFromActivityLayer()
        {
            var visualLayers = new List<IVisualLayer> { visualActivityLayer };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
                var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime, localDate, timeZone);

                var layerDetails = result.Single();
                layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
                layerDetails.Summary.Should().Be.EqualTo("9:00");
                layerDetails.Title.Should().Be.EqualTo("Phone");
                layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
                layerDetails.Meeting.Should().Be.Null();
                layerDetails.Color.Should().Be.EqualTo("0,128,0");
                layerDetails.StartPositionPercentage.Should().Be.EqualTo(0);
                layerDetails.EndPositionPercentage.Should().Be.EqualTo((17.0 - 8.0) / (19.0 - 8.0));
            }
        }

        [Test]
        public void ShouldCreatePeriodViewModelFromActivityLayerForNightShift()
        {
            localDate = new DateTime(2012, 08, 28);

            period = new DateTimePeriod(new DateTime(2012, 8, 27, 18, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2012, 8, 28, 02, 0, 0, DateTimeKind.Utc));
            visualActivityLayer = factory.CreateShiftSetupLayer(activity, period, person);

            var visualLayers = new List<IVisualLayer> { visualActivityLayer };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
	        using (mocks.Playback())
	        {
		        var visualShiftStart = TimeSpan.Zero;
		        var shiftEnd = new TimeSpan(4, 0, 0);
		        var endTimeLine = new TimeSpan(4, 0, 0);

		        minMaxTime = new TimePeriod(new TimeSpan(0, 0, 0), endTimeLine);
		        var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime, localDate, timeZone);

		        var layerDetails = result.Single();
		        layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
		        layerDetails.Summary.Should().Be.EqualTo("8:00");
		        layerDetails.Title.Should().Be.EqualTo("Phone");
		        layerDetails.TimeSpan.Should().Be.EqualTo("20:00 - 04:00 +1");
		        layerDetails.Meeting.Should().Be.Null();
		        layerDetails.Color.Should().Be.EqualTo("0,128,0");
		        layerDetails.StartPositionPercentage.Should()
			        .Be.EqualTo((visualShiftStart - TimeSpan.Zero).Ticks/(decimal) (endTimeLine - TimeSpan.Zero).Ticks);
		        layerDetails.EndPositionPercentage.Should()
			        .Be.EqualTo((shiftEnd - TimeSpan.Zero).Ticks/(decimal) (endTimeLine - TimeSpan.Zero).Ticks);
	        }
        }

        [Test]
        public void ShouldCreatePeriodViewModelFromAbsenceLayer()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            absence.SetId(Guid.NewGuid());

            var visualLayers = new List<IVisualLayer>
            {
	            factory.CreateAbsenceSetupLayer(absence, visualActivityLayer, period, Guid.NewGuid())
            };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
                var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime, localDate, timeZone);

                var layerDetails = result.Single();
                layerDetails.StyleClassName.Should().Be.EqualTo("color_FF0000");
                layerDetails.Summary.Should().Be.EqualTo("9:00");
                layerDetails.Title.Should().Be.EqualTo("Holiday");
                layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
                layerDetails.Meeting.Should().Be.Null();
                layerDetails.Color.Should().Be.EqualTo("255,0,0");
                layerDetails.StartPositionPercentage.Should().Be.EqualTo(0);
                layerDetails.EndPositionPercentage.Should().Be.EqualTo((17.0 - 8.0) / (19.0 - 8.0));
            }
        }

        [Test]
        public void ShouldCreatePeriodViewModelFromOvertimeLayer()
        {
	        var definitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Overtime",
		        MultiplicatorType.Overtime);

            ((VisualLayer)visualActivityLayer).DefinitionSet = definitionSet;
            var visualLayers = new List<IVisualLayer> { visualActivityLayer };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
                var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime, localDate, timeZone);

                var layerDetails = result.Single();
                layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
                layerDetails.Summary.Should().Be.EqualTo("9:00");
                layerDetails.Title.Should().Be.EqualTo("Phone");
                layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
                layerDetails.Meeting.Should().Be.Null();
                layerDetails.Color.Should().Be.EqualTo("0,128,0");
                layerDetails.StartPositionPercentage.Should().Be.EqualTo(0);
                layerDetails.EndPositionPercentage.Should().Be.EqualTo((17.0 - 8.0) / (19.0 - 8.0));
	            layerDetails.IsOvertime.Should().Be(true);
            }
        }

        [Test]
        public void ShouldCreatePeriodViewModelFromMeetingLayer()
        {
            var testPerson = PersonFactory.CreatePerson();
	        var meeting = new Meeting(testPerson, new[]
	        {
		        new MeetingPerson(testPerson, false)
	        }, "subj", "loc", "desc", activity, null);
            IMeetingPayload meetingPayload = new MeetingPayload(meeting);

            var visualLayers = new List<IVisualLayer> { factory.CreateMeetingSetupLayer(meetingPayload, visualActivityLayer, period) };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
                var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime, localDate, timeZone);

                var layerDetails = result.Single();
                layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
                layerDetails.Summary.Should().Be.EqualTo("9:00");
                layerDetails.Title.Should().Be.EqualTo("Phone");
                layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
                layerDetails.Meeting.Title.Should().Be.EqualTo("subj");
                layerDetails.Meeting.Location.Should().Be.EqualTo("loc");
					 layerDetails.Meeting.Description.Should().Be.EqualTo("desc");
                layerDetails.Color.Should().Be.EqualTo("0,128,0");
                layerDetails.StartPositionPercentage.Should().Be.EqualTo(0);
                layerDetails.EndPositionPercentage.Should().Be.EqualTo((17.0 - 8.0) / (19.0 - 8.0));
            }
        }

	    [Test]
	    public void ShouldCreateOvertimeAvailabilityPeriodViewModelNotSpanToTomorrow()
	    {
		    var start = new TimeSpan(12, 0, 0);
		    var end = new TimeSpan(24, 0, 1);
		    var result =
			    target.CreateOvertimeAvailabilityPeriodViewModels(
				    new OvertimeAvailability(new Person(), DateOnly.Today, start, end), null, minMaxTime).First();

		    result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
		    result.TimeSpan.Should()
			    .Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
		    result.StartPositionPercentage.Should()
			    .Be.EqualTo((decimal) (start - minMaxTime.StartTime).Ticks/(minMaxTime.EndTime - minMaxTime.StartTime).Ticks);
		    result.EndPositionPercentage.Should().Be.EqualTo(1);
		    result.IsOvertimeAvailability.Should().Be.True();
		    result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
	    }

	    [Test]
	    public void ShouldCreateOvertimeAvailabilityPeriodViewModel()
	    {
		    var start = new TimeSpan(12, 0, 0);
		    var end = new TimeSpan(13, 0, 0);
		    var result =
			    target.CreateOvertimeAvailabilityPeriodViewModels(
				    new OvertimeAvailability(new Person(), DateOnly.Today, start, end), null, minMaxTime).First();

		    result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
		    result.TimeSpan.Should()
			    .Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
		    result.StartPositionPercentage.Should()
			    .Be.EqualTo((decimal) (start - minMaxTime.StartTime).Ticks/(minMaxTime.EndTime - minMaxTime.StartTime).Ticks);
		    result.EndPositionPercentage.Should()
			    .Be.EqualTo((decimal) (end - minMaxTime.StartTime).Ticks/(minMaxTime.EndTime - minMaxTime.StartTime).Ticks);
		    result.IsOvertimeAvailability.Should().Be.True();
		    result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
	    }

	    [Test]
	    public void ShouldCreateOvertimeAvailabilityPeriodViewModelForYesterday()
	    {
		    var start = new TimeSpan(12, 0, 0);
		    var end = new TimeSpan(25, 0, 0);
		    var overtimeAvailabilityYesterday = new OvertimeAvailability(new Person(), DateOnly.Today, start, end);
		    var overtimeAvailabilityViewModel = new OvertimeAvailabilityViewModel();
		    _mapper.Stub(x => x.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(overtimeAvailabilityYesterday))
			    .Return(overtimeAvailabilityViewModel);

		    var result =
			    target.CreateOvertimeAvailabilityPeriodViewModels(null, overtimeAvailabilityYesterday, minMaxTime).First();

		    result.Title.Should().Be.EqualTo(Resources.OvertimeAvailabilityWeb);
		    result.TimeSpan.Should()
			    .Equals(TimeHelper.TimeOfDayFromTimeSpan(start, CultureInfo.CurrentCulture) + " - " +
						TimeHelper.TimeOfDayFromTimeSpan(end, CultureInfo.CurrentCulture));
		    result.StartPositionPercentage.Should().Be.EqualTo(0);
		    result.EndPositionPercentage.Should()
			    .Be.EqualTo((decimal) (end.Subtract(new TimeSpan(1, 0, 0, 0)) - minMaxTime.StartTime).Ticks/
							(minMaxTime.EndTime - minMaxTime.StartTime).Ticks);
		    result.IsOvertimeAvailability.Should().Be.True();
		    result.OvertimeAvailabilityYesterday.Should().Be.SameInstanceAs(overtimeAvailabilityViewModel);
		    result.Color.Should().Be.EqualTo(Color.Gray.ToCSV());
	    }

	    [Test]
		public void ShouldNotCreateOvertimeAvailabilityPeriodViewModelForYesterdayIfNotSpanToToday()
		{
			var start = new TimeSpan(12, 0, 0);
			var end = new TimeSpan(13, 0, 0);
		    var result = target.CreateOvertimeAvailabilityPeriodViewModels(null,
			    new OvertimeAvailability(new Person(), DateOnly.Today, start, end), minMaxTime);
			result.Should().Be.Empty();
		}

	    [TearDown]
        public void Teardown()
        {
            System.Threading.Thread.CurrentPrincipal = principalBefore;
        }
    }
}