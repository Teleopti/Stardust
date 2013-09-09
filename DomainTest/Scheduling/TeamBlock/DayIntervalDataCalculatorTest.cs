using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class DayIntervalDataCalculatorTest
	{
		private IDayIntervalDataCalculator _target;
		private MockRepository _mocks;
		private IIntervalDataCalculator _intervalDataCalculator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_intervalDataCalculator = _mocks.StrictMock<IIntervalDataCalculator>();
			_target = new DayIntervalDataCalculator(_intervalDataCalculator);
		}

		[Test]
		public void ShouldCalculateMedianDay()
		{
			var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
			var skillIntervalData1 = new SkillIntervalData(new DateTimePeriod(baseDate, baseDate.AddMinutes(15)), 10, 5, 0, 0, 0);
			var skillIntervalData2 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(1), baseDate.AddDays(1).AddMinutes(15)), 15, 10, 0, 0, 0);
			var skillIntervalData3 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(2), baseDate.AddDays(2).AddMinutes(15)), 3, 3, 0, 0, 0);
			var skillIntervalData4 = new SkillIntervalData(new DateTimePeriod(baseDate.AddMinutes(15), baseDate.AddMinutes(30)), 4, 2, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(baseDate), new[] { skillIntervalData1, skillIntervalData4 });
			dayIntervalData.Add(new DateOnly(baseDate.AddDays(1)), new[] { skillIntervalData2 });
			dayIntervalData.Add(new DateOnly(baseDate.AddDays(2)), new[] { skillIntervalData3 });

			using(_mocks.Record())
			{
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{10, 15, 3})).Return(10);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{5, 10, 3})).Return(5);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{4})).Return(4);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{2})).Return(2);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 0, 0, 0 })).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 0 })).Return(0);
			}

			using(_mocks.Playback())
			{
				var result = _target.Calculate(15, dayIntervalData);

				Assert.That(result.Count, Is.EqualTo(2));
				Assert.That(result[TimeSpan.Zero].ForecastedDemand, Is.EqualTo(10));
				Assert.That(result[TimeSpan.Zero].CurrentDemand, Is.EqualTo(5));
				Assert.That(result[TimeSpan.FromMinutes(15)].ForecastedDemand, Is.EqualTo(4));
				Assert.That(result[TimeSpan.FromMinutes(15)].CurrentDemand, Is.EqualTo(2));
			}
		}

		[Test]
		public void ShouldCalculateForOpenIntervalsOnly()
		{
			//we have a blocklenght of one day (2013-01-01), and only one open interval (01:00 - 01:15)
			var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

			var skillIntervalDataForDay1 = new SkillIntervalData(new DateTimePeriod(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)), 10, 5, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(2013, 1, 1), new List<ISkillIntervalData> { skillIntervalDataForDay1 });

			using (_mocks.Record())
			{
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{10})).Return(10);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{5})).Return(5);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 0 })).Return(0);
			}

			using (_mocks.Playback())
			{
				IDictionary<TimeSpan, ISkillIntervalData> result = _target.Calculate(15, dayIntervalData);
				Assert.That(result.Count, Is.EqualTo(1));  //No need to calculate closed intervals
				Assert.That(result.Keys.FirstOrDefault(), Is.EqualTo(new TimeSpan(1, 0, 0)));
			}
		}

		[Test]
		public void ShouldReturnCorrectValuesForForecastedDemand()
		{
			//we have a blocklenght of two days (2013-01-01 and 2013-01-02), and only one open interval (01:00 - 01:15)
			var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

			var skillIntervalDataForDay1 = new SkillIntervalData(new DateTimePeriod(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)), 10, 5, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(2013, 1, 1), new List<ISkillIntervalData> { skillIntervalDataForDay1 });

			var skillIntervalDataForDay2 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(1).AddHours(1), baseDate.AddDays(1).AddHours(1).AddMinutes(15)), 10, 5, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(2013, 1, 2), new List<ISkillIntervalData> { skillIntervalDataForDay2 });

			using (_mocks.Record())
			{
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{10, 10})).Return(10);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{5, 5})).Return(5);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 0, 0 })).Return(0);
			}

			using (_mocks.Playback())
			{
				IDictionary<TimeSpan, ISkillIntervalData> result = _target.Calculate(15, dayIntervalData);
				Assert.That(result[TimeSpan.FromHours(1)].ForecastedDemand, Is.EqualTo(10)); //median of 10 and 10
			}
		}

		[Test]
		public void ShouldReturnCorrectValuesForCurrentDemand()
		{
			//we have a blocklenght of two days (2013-01-01 and 2013-01-02), and only one open interval (01:00 - 01:15)
			var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

			var skillIntervalDataForDay1 = new SkillIntervalData(new DateTimePeriod(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)), 10, 5, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(2013, 1, 1), new List<ISkillIntervalData> { skillIntervalDataForDay1 });

			var skillIntervalDataForDay2 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(1).AddHours(1), baseDate.AddDays(1).AddHours(1).AddMinutes(15)), 10, 5, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(2013, 1, 2), new List<ISkillIntervalData> { skillIntervalDataForDay2 });

			using (_mocks.Record())
			{
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{10, 10})).Return(10);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>{5, 5})).Return(5);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 0, 0 })).Return(0);
			}

			using (_mocks.Playback())
			{
				IDictionary<TimeSpan, ISkillIntervalData> result = _target.Calculate(15, dayIntervalData);
				Assert.That(result[TimeSpan.FromHours(1)].CurrentDemand, Is.EqualTo(5)); //median of 5 and 5
			}
		}

        [Test]
        public void ShouldReturnCorrectValuesForScheduledAgents()
        {
            //we have a blocklenght of two days (2013-01-01 and 2013-01-02), and only one open interval (01:00 - 01:15)
            var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

            var skillIntervalDataForDay1 = new SkillIntervalData(new DateTimePeriod(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)), 10, 5, 5, 1, 1);
            dayIntervalData.Add(new DateOnly(2013, 1, 1), new List<ISkillIntervalData> { skillIntervalDataForDay1 });

            var skillIntervalDataForDay2 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(1).AddHours(1), baseDate.AddDays(1).AddHours(1).AddMinutes(15)), 10, 5, 5, 1, 1);
            dayIntervalData.Add(new DateOnly(2013, 1, 2), new List<ISkillIntervalData> { skillIntervalDataForDay2 });

            using (_mocks.Record())
            {
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 10, 10 })).Return(10);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 5, 5 })).Return(5);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 1, 1 })).Return(1) ;
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 1, 1 })).Return(1);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 5, 5 })).Return(5);
            }

            using (_mocks.Playback())
            {
                IDictionary<TimeSpan, ISkillIntervalData> result = _target.Calculate(15, dayIntervalData);
                Assert.That(result[TimeSpan.FromHours(1)].CurrentHeads , Is.EqualTo(5)); //median of 5 and 5
            }
        }

        [Test]
        public void ShouldReturnCorrectValuesForMinimumAgents()
        {
            //we have a blocklenght of two days (2013-01-01 and 2013-01-02), and only one open interval (01:00 - 01:15)
            var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

            var skillIntervalDataForDay1 = new SkillIntervalData(new DateTimePeriod(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)), 10, 5, 5, 7, 0);
            dayIntervalData.Add(new DateOnly(2013, 1, 1), new List<ISkillIntervalData> { skillIntervalDataForDay1 });

            var skillIntervalDataForDay2 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(1).AddHours(1), baseDate.AddDays(1).AddHours(1).AddMinutes(15)), 10, 5, 5, 8, 0);
            dayIntervalData.Add(new DateOnly(2013, 1, 2), new List<ISkillIntervalData> { skillIntervalDataForDay2 });

            using (_mocks.Record())
            {
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 10, 10 })).Return(10);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 5, 5 })).Return(5);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 7, 8 })).Return(7.5);
	            Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).Return(0);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 5, 5 })).Return(5);
            }

            using (_mocks.Playback())
            {
                IDictionary<TimeSpan, ISkillIntervalData> result = _target.Calculate(15, dayIntervalData);
                Assert.That(result[TimeSpan.FromHours(1)].MinimumHeads , Is.EqualTo(7.5)); //median of 5 and 5
            }
        }

        [Test]
        public void ShouldReturnCorrectValuesForMaximumAgents()
        {
            //we have a blocklenght of two days (2013-01-01 and 2013-01-02), and only one open interval (01:00 - 01:15)
            var dayIntervalData = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

            var skillIntervalDataForDay1 = new SkillIntervalData(new DateTimePeriod(baseDate.AddHours(1), baseDate.AddHours(1).AddMinutes(15)), 10, 5, 5, 7, 3);
            dayIntervalData.Add(new DateOnly(2013, 1, 1), new List<ISkillIntervalData> { skillIntervalDataForDay1 });

            var skillIntervalDataForDay2 = new SkillIntervalData(new DateTimePeriod(baseDate.AddDays(1).AddHours(1), baseDate.AddDays(1).AddHours(1).AddMinutes(15)), 10, 5, 5, 8, 9);
            dayIntervalData.Add(new DateOnly(2013, 1, 2), new List<ISkillIntervalData> { skillIntervalDataForDay2 });

            using (_mocks.Record())
            {
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 10, 10 })).Return(10);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 5, 5 })).Return(5);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 7, 8 })).Return(7.5);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 3, 9 })).Return(6);
                Expect.Call(_intervalDataCalculator.Calculate(new List<double> { 5, 5 })).Return(5);
            }

            using (_mocks.Playback())
            {
                IDictionary<TimeSpan, ISkillIntervalData> result = _target.Calculate(15, dayIntervalData);
                Assert.That(result[TimeSpan.FromHours(1)].MaximumHeads , Is.EqualTo(6)); //median of 5 and 5
            }
        }
	}
}
