using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TwoDaysIntervalGeneratorTest
    {
        private ITwoDaysIntervalGenerator _target;

        [SetUp]
        public void Setup()
        {
            _target = new TwoDaysIntervalGenerator() ;
        }

        [Test]
        public void ShouldAddDataFromDayAfter()
        {
            var skillIntervalData0 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 22, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 01, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2013, 10, 02, 1, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2013, 10, 02, 2, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);

            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			list.Add(new DateOnly(2013, 10, 01), new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1 });
            list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData> { skillIntervalData2, skillIntervalData3 });

            var result = _target.GenerateTwoDaysInterval(list);

            Assert.AreEqual(result.Count, 1);
			Assert.AreEqual(result[new DateOnly(2013, 10, 01)].Count, 4);
			Assert.AreEqual(result[new DateOnly(2013, 10, 01)][TimeSpan.FromHours(23)].ForecastedDemand, 4);
			Assert.AreEqual(result[new DateOnly(2013, 10, 01)][TimeSpan.FromDays(1)].ForecastedDemand, 5);

        }

        [Test]
        public void ShouldMapFromUTC()
        {
			var skillIntervalData0 =
							new SkillIntervalData(
								new DateTimePeriod(new DateTime(2013, 10, 01, 23, 30, 0, DateTimeKind.Utc),
												   new DateTime(2013, 10, 01, 23, 45, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
			var skillIntervalData1 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 01, 23, 45, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
			var skillIntervalData2 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 0, 15, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
			var skillIntervalData3 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 0, 15, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 0, 30, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);

			IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1, skillIntervalData2, skillIntervalData3 });
			list.Add(new DateOnly(2013, 10, 03), new List<ISkillIntervalData>());

			var result = _target.GenerateTwoDaysInterval(list);

			Assert.AreEqual(result.Count, 1);
			Assert.AreEqual(result[new DateOnly(2013, 10, 02)].Count, 4);
	        Assert.AreEqual(result[new DateOnly(2013, 10, 02)][new TimeSpan(-1, 23, 45, 0)].ForecastedDemand, 4);
			Assert.AreEqual(result[new DateOnly(2013, 10, 02)][TimeSpan.Zero].ForecastedDemand, 5);

        }
    }
}
