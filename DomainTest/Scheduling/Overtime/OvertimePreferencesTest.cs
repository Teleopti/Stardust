using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;


namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class OvertimePreferencesTest
    {
        private IOvertimePreferences _target;

        [SetUp]
        public void Setup()
        {
            _target = new OvertimePreferences();
        }


        [Test]
        public void VerifyDoNotBreakMaxWorkPerWeek()
        {
            _target.AllowBreakMaxWorkPerWeek = true;
            Assert.IsTrue(_target.AllowBreakMaxWorkPerWeek );
        }

        [Test]
        public void VerifyDoNotBreakNightlyRest()
        {
            _target.AllowBreakNightlyRest = true;
            Assert.IsTrue(_target.AllowBreakNightlyRest );
        }

        [Test]
        public void VerifyDoNotBreakWeeklyRest()
        {
            _target.AllowBreakWeeklyRest  = true;
            Assert.IsTrue(_target.AllowBreakWeeklyRest);
        }

        [Test]
        public void VerifyOvertimeToFrom()
        {
            _target.SelectedTimePeriod   = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
            Assert.AreEqual(_target.SelectedTimePeriod , new TimePeriod(TimeSpan.FromHours(8),TimeSpan.FromHours(10) ));
        }

        [Test]
        public void VerifyScheduleTag()
        {
            var scheduleTag = new ScheduleTag();
            scheduleTag.Description = "test";
            _target.ScheduleTag = scheduleTag;
            _target.SelectedTimePeriod = new TimePeriod(TimeSpan.FromHours(8),TimeSpan.FromHours(10));
            Assert.AreEqual(_target.ScheduleTag.Description, "test");
        }

        [Test]
        public void VerifySkillActivity()
        {
            _target.SkillActivity = new Activity("thisActivity");
            Assert.AreEqual(_target.SkillActivity.Name ,"thisActivity");
        }

        [Test]
        public void VerifyOvertimeType()
        {
            _target.OvertimeType = new MultiplicatorDefinitionSet("this",MultiplicatorType.Overtime );
            Assert.AreEqual(_target.OvertimeType.Name,"this" );
        }

        [Test]
        public void VerifyAvailableAgentsOnlyMapped()
        {
            _target.AvailableAgentsOnly = true;
            Assert.IsTrue(_target.AvailableAgentsOnly);
        }
    }
}
