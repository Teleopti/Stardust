using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class DayIntervalDataCalculatorTest
    {
        private IDayIntervalDataCalculator _target;
        private MedianCalculatorForDays _medianCalculatorForDays;
        private ITwoDaysIntervalGenerator _twoDayIntervalCalculator;
        private IMedianCalculatorForSkillInterval _medianCalculatorForSkillInterval;
        private IIntervalDataCalculator _intervalDataCalculator;

        [SetUp]
        public void Setup()
        {
            _intervalDataCalculator = new IntervalDataMedianCalculator();
            _medianCalculatorForSkillInterval = new MedianCalculatorForSkillInterval(_intervalDataCalculator);
            _medianCalculatorForDays = new MedianCalculatorForDays(_medianCalculatorForSkillInterval);
            _twoDayIntervalCalculator=new TwoDaysIntervalGenerator();
            _target = new DayIntervalDataCalculator(_medianCalculatorForDays,_twoDayIntervalCalculator);
        }

        [Test]
        public void ShouldReturnNull()
        {
            var result = _target.Calculate(null, new DateOnly());
            Assert.IsNull(result);
        }

		[Test]
		public void VerifyEmptySkillIntervalIsConsideredForMedianCalculation()
		{

			var skillIntervalData1 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 15, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 16, 0, 0, DateTimeKind.Utc)), 4, 4, 0, null, null);

			var skillIntervalData2 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 16, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 17, 0, 0, DateTimeKind.Utc)), 6, 6, 0, null, null);
			var skillIntervalData3 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 17, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 18, 0, 0, DateTimeKind.Utc)), 8, 8, 0, null, null);
			var skillIntervalData4 =
				new SkillIntervalData(
					new DateTimePeriod(new DateTime(2013, 10, 02, 18, 0, 0, DateTimeKind.Utc),
									   new DateTime(2013, 10, 02, 19, 0, 0, DateTimeKind.Utc)), 11, 11, 0, null, null);
			IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			list.Add(new DateOnly(2013, 10, 01), new List<ISkillIntervalData>());
			list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData> { skillIntervalData1, skillIntervalData2, skillIntervalData3, skillIntervalData4 });
			list.Add(new DateOnly(2013, 10, 03), new List<ISkillIntervalData>());


			var result = _target.Calculate(list, new DateOnly(2013, 10, 02));
			Assert.AreEqual(result.Count, 4);
			Assert.AreEqual(result[skillIntervalData1.Period.StartDateTime].ForecastedDemand, 4);
			Assert.AreEqual(result[skillIntervalData2.Period.StartDateTime].ForecastedDemand, 6);
			Assert.AreEqual(result[skillIntervalData3.Period.StartDateTime].ForecastedDemand, 8);
			Assert.AreEqual(result[skillIntervalData4.Period.StartDateTime].ForecastedDemand, 11);

		}


		[Test]
		public void ShouldReturnEmptySkillIntervalIsConsideredForMedianCalculation()
		{
			IDictionary<DateOnly, IList<ISkillIntervalData>> list = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			list.Add(new DateOnly(2013, 10, 01), new List<ISkillIntervalData>());
			list.Add(new DateOnly(2013, 10, 02), new List<ISkillIntervalData>());

			var result = _target.Calculate(list, new DateOnly(2013, 10, 01));
			Assert.That(result.Count, Is.EqualTo(0));
		}
    }
}
