using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
    [TestFixture, SetCulture("sv-SE"), SetUICulture("sv-SE")]
    public class PeriodViewModelFactoryTest
    {
        private IPeriodViewModelFactory target;
        private MockRepository mocks;
        private ICccTimeZoneInfo timeZone;
        private IVisualLayerCollection visualLayerCollection;
        private IPrincipal principalBefore;
        private DateTimePeriod period;
        private IVisualLayerFactory factory;
        private IActivity activity;
        private IVisualLayer visualActivityLayer;
    	private TimePeriod minMaxTime;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            
            timeZone = CccTimeZoneInfoFactory.StockholmTimeZoneInfo();

            setPrincipal();
            
            visualLayerCollection = mocks.DynamicMock<IVisualLayerCollection>();
            period = new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc));
            factory = new VisualLayerFactory();
            activity = ActivityFactory.CreateActivity("Phone");
            activity.SetId(Guid.NewGuid());
			minMaxTime = new TimePeriod(8, 0, 19, 0);

            visualActivityLayer = factory.CreateShiftSetupLayer(activity, period);

            target = new PeriodViewModelFactory();
        }

        private void setPrincipal()
        {
            principalBefore = System.Threading.Thread.CurrentPrincipal;
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone(timeZone);
            System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
					 new TeleoptiIdentity("test", null, null, null, AuthenticationTypeOption.Unknown), person);
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
				var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime);

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
        public void ShouldCreatePeriodViewModelFromAbsenceLayer()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            absence.SetId(Guid.NewGuid());

            var visualLayers = new List<IVisualLayer> { factory.CreateAbsenceSetupLayer(absence, visualActivityLayer, period) };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
				var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime);

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
            var definitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);

            ((VisualLayer) visualActivityLayer).DefinitionSet = definitionSet;
            var visualLayers = new List<IVisualLayer> { visualActivityLayer };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
				var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime);

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
        public void ShouldCreatePeriodViewModelFromMeetingLayer()
        {
            var person = PersonFactory.CreatePerson();
            var meeting = new Meeting(person, new[] {new MeetingPerson(person, false)}, "subj", "loc", "desc", activity,
                                      null);
            IMeetingPayload meetingPayload = new MeetingPayload(meeting);

            var visualLayers = new List<IVisualLayer> { factory.CreateMeetingSetupLayer(meetingPayload, visualActivityLayer, period) };
            using (mocks.Record())
            {
                Expect.Call(visualLayerCollection.GetEnumerator()).Return(visualLayers.GetEnumerator());
            }
            using (mocks.Playback())
            {
				var result = target.CreatePeriodViewModels(visualLayerCollection, minMaxTime);

                var layerDetails = result.Single();
                layerDetails.StyleClassName.Should().Be.EqualTo("color_008000");
                layerDetails.Summary.Should().Be.EqualTo("9:00");
                layerDetails.Title.Should().Be.EqualTo("Phone");
                layerDetails.TimeSpan.Should().Be.EqualTo("08:00 - 17:00");
                layerDetails.Meeting.Title.Should().Be.EqualTo("subj");
                layerDetails.Meeting.Location.Should().Be.EqualTo("loc");
				layerDetails.Color.Should().Be.EqualTo("0,128,0");
				layerDetails.StartPositionPercentage.Should().Be.EqualTo(0);
				layerDetails.EndPositionPercentage.Should().Be.EqualTo((17.0 - 8.0) / (19.0 - 8.0));
            }
        }

        [TearDown]
        public void Teardown()
        {
            System.Threading.Thread.CurrentPrincipal = principalBefore;
        }
    }
}