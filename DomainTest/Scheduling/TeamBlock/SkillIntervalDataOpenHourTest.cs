using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class SkillIntervalDataOpenHourTest
    {
        private ISkillIntervalDataOpenHour _target;

        [SetUp]
        public void Setup()
        {
            _target = new SkillIntervalDataOpenHour();
        }

        [Test]
        public void ExtractOpenHours()
        {
            ISkillIntervalData skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 17, 08, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 17, 09, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);

            ISkillIntervalData skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 17, 09, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 17, 10, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);
            ISkillIntervalData skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 17, 10, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 17, 11, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);
            IList<ISkillIntervalData> skillIntervalList = new List<ISkillIntervalData> { skillIntervalData1, skillIntervalData2, skillIntervalData3 };
            var result = _target.GetOpenHours(skillIntervalList);
            Assert.AreEqual(result, new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(11)));
        }

        [Test]
        public void ExtractOpenHoursForNightlyShifts()
        {
            ISkillIntervalData skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 17, 22, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 17, 23, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);

            ISkillIntervalData skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 17, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 18, 0, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);
            ISkillIntervalData skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 18, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 18, 1, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);
            ISkillIntervalData skillIntervalData4 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 18, 1, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 18, 2, 0, 0, DateTimeKind.Utc)), 10, 11, 12, 13, 14);
            IList<ISkillIntervalData> skillIntervalList = new List<ISkillIntervalData> { skillIntervalData1, skillIntervalData2, skillIntervalData3,skillIntervalData4 };
            var result = _target.GetOpenHours(skillIntervalList);
            Assert.AreEqual(result, new TimePeriod(TimeSpan.FromHours(22), new TimeSpan(1,2,0,0)));
        }

      
    }

    
}
