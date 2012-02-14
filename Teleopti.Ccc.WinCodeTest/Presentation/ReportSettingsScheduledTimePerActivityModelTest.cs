using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Reporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsScheduledTimePerActivityModelTest
    {
        private ReportSettingsScheduledTimePerActivityModel _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportSettingsScheduledTimePerActivityModel();

        }

        [Test]
        public void VerifyProperties()
        {
            IScenario scenario = new Scenario("scenario");
            DateOnlyPeriod period = new DateOnlyPeriod();
            IList<IPerson> persons = new List<IPerson>();
            ICccTimeZoneInfo timeZone = new CccTimeZoneInfo();
            IList<IActivity> activities = new List<IActivity>();

            _target.Scenario = scenario;
            _target.Period = period;
            _target.Persons = persons;
            _target.TimeZone = timeZone;
            _target.Activities = activities;

            Assert.AreEqual(scenario, _target.Scenario);
            Assert.AreEqual(period, _target.Period);
            Assert.AreEqual(persons, _target.Persons);
            Assert.AreEqual(timeZone, _target.TimeZone);
            Assert.AreEqual(activities, _target.Activities);
        }
    }
}
