using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
    [TestFixture]
    public class VisualLayerCalendarDayExtractorTest
    {
        private VisualLayerCalendarDayExtractor _target;
        private IActivity _activity;
        private IPerson _person;
        private TimeZoneInfo _timeZone;
        List<IVisualLayer> _visualLayerCollection;
        private IPrincipal _principalBefore;

        [SetUp]
        public void Setup()
        {
            _timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            setPrincipal();
        }

        private void CreateLayers(DateTimePeriod period)
        {
            _activity = ActivityFactory.CreateActivity("Phone");
            var factory = new VisualLayerFactory();
            var visualActivityLayer = factory.CreateShiftSetupLayer(_activity, period, _person);
            _visualLayerCollection = new List<IVisualLayer> {visualActivityLayer};
        }

        private void setPrincipal()
        {
            _principalBefore = System.Threading.Thread.CurrentPrincipal;
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
                new TeleoptiIdentity("test", null, null, null), _person);
        }

        [Test]
        public void ShouldReturnVisualPeriodWhenMidnightShiftLayerStartsYesterday()
        {
            var period = new DateTimePeriod(new DateTime(2012, 8, 27, 18, 0, 0, DateTimeKind.Utc),
                                         new DateTime(2012, 8, 28, 02, 0, 0, DateTimeKind.Utc));
            CreateLayers(period);

            var localDate = new DateTime(2012, 08, 28, 0, 0, 0,DateTimeKind.Local);
            _target = new VisualLayerCalendarDayExtractor();
            var layersWithVisualPeriods = _target.CreateVisualPeriods(localDate, _visualLayerCollection, _timeZone);

            VisualLayerForWebDisplay layer = layersWithVisualPeriods.Single();
            layer.VisualPeriod.StartDateTime.Date.Should().Be.EqualTo(localDate.AddDays(-1));
            layer.VisualPeriod.EndDateTime.Date.Should().Be.EqualTo(localDate);
            layer.VisualPeriod.StartDateTime.TimeOfDay.Should().Be.EqualTo(new TimeSpan(22, 0, 0));
            layer.VisualPeriod.EndDateTime.TimeOfDay.Should().Be.EqualTo(new TimeSpan(2, 0, 0));
        }

        [Test]
        public void ShouldReturnVisualPeriodWhenMidnightShiftLayerStartsToday()
        {
            var period = new DateTimePeriod(new DateTime(2012, 8, 27, 18, 0, 0, DateTimeKind.Utc),
                                         new DateTime(2012, 8, 28, 02, 0, 0, DateTimeKind.Utc));
            CreateLayers(period);
            var localDate = new DateTime(2012, 08, 27, 0, 0, 0, DateTimeKind.Local);
            _target = new VisualLayerCalendarDayExtractor();
            var layersWithVisualPeriods = _target.CreateVisualPeriods(localDate, _visualLayerCollection, _timeZone);

            VisualLayerForWebDisplay layer = layersWithVisualPeriods.Single();
            layer.VisualPeriod.StartDateTime.Date.Should().Be.EqualTo(localDate);
            layer.VisualPeriod.EndDateTime.Date.Should().Be.EqualTo(localDate);
            layer.VisualPeriod.StartDateTime.TimeOfDay.Should().Be.EqualTo(new TimeSpan(18, 0, 0));
            layer.VisualPeriod.EndDateTime.TimeOfDay.Should().Be.EqualTo(new TimeSpan(21, 59, 59));
        }

        [Test]
        public void ShouldReturnVisualPeriodWhenMidnightShiftLayerStartsTomorrow()
        {
            var period = new DateTimePeriod(new DateTime(2012, 8, 27, 22, 0, 0, DateTimeKind.Utc),
                                         new DateTime(2012, 8, 28, 02, 0, 0, DateTimeKind.Utc));
            CreateLayers(period);
            var localDate = new DateTime(2012, 08, 27, 0, 0, 0, DateTimeKind.Local);

            _target = new VisualLayerCalendarDayExtractor();
            var layersWithVisualPeriods = _target.CreateVisualPeriods(localDate, _visualLayerCollection, _timeZone);

            layersWithVisualPeriods.Count.Should().Be.EqualTo(0);
        }


        [TearDown]
        public void Teardown()
        {
            System.Threading.Thread.CurrentPrincipal = _principalBefore;
        }
    }
}