using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
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
			var skillIntervalData2 = new SkillIntervalData(new DateTimePeriod(baseDate, baseDate.AddMinutes(15)), 15, 10, 0, 0, 0);
			var skillIntervalData3 = new SkillIntervalData(new DateTimePeriod(baseDate, baseDate.AddMinutes(15)), 3, 3, 0, 0, 0);
			var skillIntervalData4 = new SkillIntervalData(new DateTimePeriod(baseDate.AddMinutes(15), baseDate.AddMinutes(30)), 4, 2, 0, 0, 0);
			dayIntervalData.Add(new DateOnly(baseDate), new[] { skillIntervalData1, skillIntervalData2, skillIntervalData3, skillIntervalData4 });

			using(_mocks.Record())
			{
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).IgnoreArguments().Return(10).Repeat.Times(1);
				Expect.Call(_intervalDataCalculator.Calculate(new List<double>())).IgnoreArguments().Return(4).Repeat.Times(191);
			}

			using(_mocks.Playback())
			{
				var result = _target.Calculate(15, dayIntervalData);

				Assert.That(result.Count, Is.EqualTo(192));
				Assert.That(result[TimeSpan.Zero].ForecastedDemand, Is.EqualTo(10));
				Assert.That(result[TimeSpan.Zero].CurrentDemand, Is.EqualTo(0));
				Assert.That(result[TimeSpan.FromMinutes(15)].ForecastedDemand, Is.EqualTo(4));
				Assert.That(result[TimeSpan.FromMinutes(15)].CurrentDemand, Is.EqualTo(0));
			}
		}
	}
}
