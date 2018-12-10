using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ScheduleTagging
{
    [TestFixture]
    public class ScheduleTagSetterTest
    {
        private IScheduleTagSetter _target;
        private IScheduleTag _tag;
        private IList<IScheduleDay> _scheduleParts;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;

        [SetUp]
        public void Setup()
        {
            _tag = new ScheduleTag();
            _tag.Description = "basic";
            _target = new ScheduleTagSetter(_tag);
            DateTime startDateTime = new DateTime(2011, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period = new DateTimePeriod(startDateTime, startDateTime.AddDays(10));
            var partFactory = new SchedulePartFactoryForDomain(PersonFactory.CreatePerson(),
                                                               ScenarioFactory.CreateScenarioAggregate(), period,
                                                               SkillFactory.CreateSkill("hej"));
            _scheduleDay1 = partFactory.CreatePart();
            _scheduleDay2 = partFactory.CreatePart();
            _scheduleParts = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};
        }

        [Test]
        public void ShouldSetSpecifiedTagIfModifierIsScheduler()
        {
            ScheduleModifier modifier = ScheduleModifier.Scheduler;
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldSetSpecifiedTagIfModifierIsAutomaticScheduling()
        {
            ScheduleModifier modifier = ScheduleModifier.AutomaticScheduling;
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldSetSameTagIfModifierIsMessageBroker()
        {
            ScheduleModifier modifier = ScheduleModifier.MessageBroker;
            _target.SetTagOnScheduleDays(ScheduleModifier.Scheduler, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            _tag = new ScheduleTag();
            _tag.Description = "mb";
            _target = new ScheduleTagSetter(_tag);
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldSetSpecifiedTagIfModifierIsRequest()
        {
            ScheduleModifier modifier = ScheduleModifier.Request;
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldSetSameTagIfModifierIsUndo()
        {
            ScheduleModifier modifier = ScheduleModifier.UndoRedo;
            _target.SetTagOnScheduleDays(ScheduleModifier.Scheduler, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            _tag = new ScheduleTag();
            _tag.Description = "mb";
            _target = new ScheduleTagSetter(_tag);
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldSetSameTagIfTypeIsKeepOriginalScheduleTag()
        {
            ScheduleModifier modifier = ScheduleModifier.UndoRedo;
            _target.SetTagOnScheduleDays(ScheduleModifier.Scheduler, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            _target = new ScheduleTagSetter(KeepOriginalScheduleTag.Instance);
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldKeepTagIfTypeIsKeepOriginalScheduleTagAndModifierIsSchedule()
        {
            const ScheduleModifier modifier = ScheduleModifier.Scheduler;
            _target.SetTagOnScheduleDays(ScheduleModifier.Scheduler, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            _target = new ScheduleTagSetter(KeepOriginalScheduleTag.Instance);
            _target.SetTagOnScheduleDays(modifier, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
        }

        [Test]
        public void ShouldBeAbleToResetTag()
        {
            _target.SetTagOnScheduleDays(ScheduleModifier.Scheduler, _scheduleParts);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("basic", _scheduleDay1.ScheduleTag().Description);
            _tag = new ScheduleTag();
            _tag.Description = "mb";
            _target.ChangeTagToSet(_tag);
            _target.SetTagOnScheduleDays(ScheduleModifier.Scheduler, _scheduleParts);
            Assert.AreEqual("mb", _scheduleDay1.ScheduleTag().Description);
            Assert.AreEqual("mb", _scheduleDay1.ScheduleTag().Description);
        }
    }
}