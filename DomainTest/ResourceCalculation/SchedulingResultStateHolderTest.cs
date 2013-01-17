﻿using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SchedulingResultStateHolderTest
    {
        private DateTimePeriod _period;
        //private bool eventRaised;

        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void VerifyProperties()
        {
            Skill skill = new Skill("skilling me softly", "with his song", Color.Red, 50, new SkillTypePhone(new Description("amfal"), ForecastSource.InboundTelephony));
            SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period, skill);
            IList<ISkill> list = target.VisibleSkills;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, target.Skills.Count);
            Assert.IsNotNull(target.PersonsInOrganization);
            Assert.IsNotNull(target.Schedules);
            Assert.IsNotNull(target.SkillDays);
            Assert.IsNotNull(target.SkillStaffPeriodHolder);
        }


        [Test]
        public void VerifySkillDayOnDate()
        {
            MockRepository mocks = new MockRepository();
            DateOnly date1 = new DateOnly(2008, 2, 2);
            DateOnly date2 = new DateOnly(2008, 2, 3);
            DateOnly date3 = new DateOnly(2008, 2, 4);
            IList<DateOnly> dateOnlys = new List<DateOnly> { date1, date2 };
            ISkill skill = mocks.StrictMock<ISkill>();

            ISkillDay skillDay1 = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay2 = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay3 = mocks.StrictMock<ISkillDay>();

            IList<ISkillDay> lst = new List<ISkillDay> { skillDay1, skillDay2, skillDay3 };

            SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period, skill, lst);

            using (mocks.Record())
            {
                Expect.Call(skillDay1.CurrentDate).Return(date1);
                Expect.Call(skillDay2.CurrentDate).Return(date2);
                Expect.Call(skillDay3.CurrentDate).Return(date3);

            }
            using (mocks.Playback())
            {
                IList<ISkillDay> ret = target.SkillDaysOnDateOnly(dateOnlys);
                Assert.IsNotNull(ret);
                Assert.AreEqual(2, ret.Count);
            }
        }

        [Test]
        public void VerifyNewSkillStaffPeriodHolderIsCreatedWhenSettingSkillDays()
        {
            Skill skill = new Skill("skilling me softly", "with his song", Color.Red, 50, new SkillTypePhone(new Description("amfal"), ForecastSource.InboundTelephony));
            SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period, skill);
            var previousHolder = target.SkillStaffPeriodHolder;
            target.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
            Assert.AreNotSame(previousHolder, target.SkillStaffPeriodHolder);
        }

		//[Test]
		//public void VerifyResourceChangedEvent()
		//{
		//    SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period);

		//    target.ResourcesChanged += target_ResourcesChanged;
		//    eventRaised = false;
		//    target.OnResourcesChanged(new List<DateOnly> { new DateOnly(_period.LocalStartDateTime) });
		//    Assert.IsTrue(eventRaised);
		//}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void VisibleSkillsShouldContainMaxSeatSkillsAndNonBlendSkills()
		{
			ISkill normal = SkillFactory.CreateSiteSkill("normal");
			ISkill maxSeat = SkillFactory.CreateSiteSkill("maxseat");
			ISkill nonBlend = SkillFactory.CreateNonBlendSkill("nonBlend");
			SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period);
			target.Skills.Add(normal);
			target.Skills.Add(maxSeat);
			target.Skills.Add(nonBlend);
			IList<ISkill> result = target.VisibleSkills;
			Assert.AreEqual(3, result.Count);
		}

		//void target_ResourcesChanged(object sender, ResourceChangedEventArgs e)
		//{
		//    eventRaised = true;
		//}

    }
}
