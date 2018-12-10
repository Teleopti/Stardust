using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


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
        public void ShouldAddDataFromDayAfterButNotForLastDate()
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
	        list.Add(new DateOnly(2013, 10, 03), new List<ISkillIntervalData>());

            var result = _target.GenerateTwoDaysInterval(list);

            Assert.AreEqual(result.Count, 2);
			Assert.AreEqual(result[new DateOnly(2013, 10, 01)].Count, 4);
			Assert.AreEqual(result[new DateOnly(2013, 10, 02)].Count, 2);
			Assert.AreEqual(result[new DateOnly(2013, 10, 01)][skillIntervalData1.Period.StartDateTime].ForecastedDemand, 4);
			Assert.AreEqual(result[new DateOnly(2013, 10, 01)][skillIntervalData2.Period.StartDateTime].ForecastedDemand, 5);

        }

        [Test]
        public void ShouldMapOvernightSkillInterval()
        {
            var today = new DateOnly(2011, 04, 29);
            var skillIntervalData0 =
                            new SkillIntervalData(
                                new DateTimePeriod(new DateTime(2011, 04, 29, 22, 0, 0, DateTimeKind.Utc),
                                                   new DateTime(2011, 04, 29, 23, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 04, 29, 23, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 04, 30, 00, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 04, 30, 00, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 04, 30, 01, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 04, 30, 01, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 04, 30, 02, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);


            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            list.Add(today, new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1 });
            list.Add(today.AddDays(1), new List<ISkillIntervalData> { skillIntervalData2, skillIntervalData3 });
			list.Add(today.AddDays(2), new List<ISkillIntervalData>());

            var result = _target.GenerateTwoDaysInterval(list);

            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(4, result[today].Count);
			Assert.AreEqual(3, result[today][skillIntervalData0.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(4, result[today][skillIntervalData1.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(5, result[today][skillIntervalData2.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(6, result[today][skillIntervalData3.Period.StartDateTime].ForecastedDemand);

            Assert.AreEqual(2, result[today.AddDays(1)].Count);
			Assert.AreEqual(5, result[today.AddDays(1)][skillIntervalData2.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(6, result[today.AddDays(1)][skillIntervalData3.Period.StartDateTime].ForecastedDemand);
        }

        [Test]
        public void ShouldMapAllSkillInterval()
        {
            var today = new DateOnly(2011, 04, 29);
            var skillIntervalData0 =
                            new SkillIntervalData(
                                new DateTimePeriod(new DateTime(2011, 04, 29, 10, 0, 0, DateTimeKind.Utc),
                                                   new DateTime(2011, 04, 29, 11, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
            var skillIntervalData1 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 04, 30, 10, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 04, 30, 11, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 05, 01, 10, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 05, 01, 11, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 05, 02, 10, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 05, 02, 11, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);


            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            list.Add(today, new List<ISkillIntervalData> { skillIntervalData0 });
            list.Add(today.AddDays(1), new List<ISkillIntervalData> { skillIntervalData1 });
            list.Add(today.AddDays(2), new List<ISkillIntervalData> { skillIntervalData2 });
            list.Add(today.AddDays(3), new List<ISkillIntervalData> { skillIntervalData3 });
			list.Add(today.AddDays(4), new List<ISkillIntervalData>());

            var result = _target.GenerateTwoDaysInterval(list);

            Assert.AreEqual(result.Count,4);
            Assert.AreEqual(2, result[today].Count);
			Assert.AreEqual(3, result[today][skillIntervalData0.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(4, result[today][skillIntervalData1.Period.StartDateTime].ForecastedDemand);

            Assert.AreEqual(2, result[today.AddDays(1)].Count);
			Assert.AreEqual(4, result[today.AddDays(1)][skillIntervalData1.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(5, result[today.AddDays(1)][skillIntervalData2.Period.StartDateTime].ForecastedDemand);
            
            Assert.AreEqual(2, result[today.AddDays(2)].Count);
			Assert.AreEqual(5, result[today.AddDays(2)][skillIntervalData2.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(6, result[today.AddDays(2)][skillIntervalData3.Period.StartDateTime].ForecastedDemand);

            Assert.AreEqual(1, result[today.AddDays(3)].Count);
			Assert.AreEqual(6, result[today.AddDays(3)][skillIntervalData3.Period.StartDateTime].ForecastedDemand);
        }

        [Test]
        public void ShouldMapSkillIntervalWithMissingDays()
        {
            var today = new DateOnly(2011, 04, 29);
            var skillIntervalData0 =
                            new SkillIntervalData(
                                new DateTimePeriod(new DateTime(2011, 04, 29, 10, 0, 0, DateTimeKind.Utc),
                                                   new DateTime(2011, 04, 29, 11, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
            var skillIntervalData2 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 05, 01, 10, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 05, 01, 11, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
            var skillIntervalData3 =
                new SkillIntervalData(
                    new DateTimePeriod(new DateTime(2011, 05, 02, 10, 0, 0, DateTimeKind.Utc),
                                       new DateTime(2011, 05, 02, 11, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);


            IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            list.Add(today, new List<ISkillIntervalData> { skillIntervalData0 });
            list.Add(today.AddDays(2), new List<ISkillIntervalData> { skillIntervalData2 });
            list.Add(today.AddDays(3), new List<ISkillIntervalData> { skillIntervalData3 });
			list.Add(today.AddDays(4), new List<ISkillIntervalData>());

            var result = _target.GenerateTwoDaysInterval(list);

            Assert.AreEqual(result.Count, 3);
            
            Assert.AreEqual(1, result[today].Count);
			Assert.AreEqual(3, result[today][skillIntervalData0.Period.StartDateTime].ForecastedDemand);
            
            Assert.AreEqual(2, result[today.AddDays(2)].Count);
			Assert.AreEqual(5, result[today.AddDays(2)][skillIntervalData2.Period.StartDateTime].ForecastedDemand);
			Assert.AreEqual(6, result[today.AddDays(2)][skillIntervalData3.Period.StartDateTime].ForecastedDemand);
            
            Assert.AreEqual(1, result[today.AddDays(3)].Count);
			Assert.AreEqual(6, result[today.AddDays(3)][skillIntervalData3.Period.StartDateTime].ForecastedDemand);
        }

		[Test]
		public void ShouldHandleChangeToWinterTimeWhenOneHourRepeat()
		{
			var today = new DateOnly(2011, 04, 29);
			var skillIntervalData0 =
							new SkillIntervalData(
								new DateTimePeriod(new DateTime(2011, 04, 29, 10, 0, 0, DateTimeKind.Utc),
												   new DateTime(2011, 04, 29, 11, 0, 0, DateTimeKind.Utc)), 3, 3, 0, null, null);
			var skillIntervalData1 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2011, 04, 29, 10, 0, 0, DateTimeKind.Utc),
									   new DateTime(2011, 04, 29, 11, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);
			var skillIntervalData2 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2011, 04, 30, 10, 0, 0, DateTimeKind.Utc),
									   new DateTime(2011, 04, 30, 11, 0, 0, DateTimeKind.Utc)), 5, 5, 0, null, null);
			var skillIntervalData3 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2011, 04, 30, 10, 0, 0, DateTimeKind.Utc),
									   new DateTime(2011, 04, 30, 11, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);


			IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			list.Add(today, new List<ISkillIntervalData> { skillIntervalData0, skillIntervalData1 });
			list.Add(today.AddDays(1), new List<ISkillIntervalData> { skillIntervalData2, skillIntervalData3 });
			list.Add(today.AddDays(2), new List<ISkillIntervalData>());

			var result = _target.GenerateTwoDaysInterval(list);

			Assert.AreEqual(result.Count, 2);
			Assert.AreEqual(2, result[today].Count);
			Assert.AreEqual(3, result[today][skillIntervalData0.Period.StartDateTime].ForecastedDemand);

			Assert.AreEqual(1, result[today.AddDays(1)].Count);
			Assert.AreEqual(5, result[today.AddDays(1)][skillIntervalData2.Period.StartDateTime].ForecastedDemand);
		}
    }
}
