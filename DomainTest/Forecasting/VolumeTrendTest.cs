using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class VolumeTrendTest
    {
        private VolumeTrend _target;
        private readonly IList<double> _callCollection = new List<double>
                                                    {
                                                        166.6666667,
                                                        375,
                                                        266.6666667,
                                                        400,
                                                        555.5555556,
                                                        500,
                                                        500,
                                                        1400,
                                                        2000,
                                                        2400,
                                                        1200,
                                                        1400,
                                                        1333.333333
                                                    };
        private Percent _trendDayFactor = new Percent(0.02);

        [SetUp]
        public void Setup()
        {
            _target = new VolumeTrend(_callCollection);
        }

        [Test]
        public void VerifyPropertiesWorks()
        {
            Assert.IsTrue(_target.Start.Key > 0);
            Assert.IsTrue(_target.Start.Value > 0);
            Assert.IsTrue(_target.End.Key > 0);
            Assert.IsTrue(_target.End.Value != 0);
        }
        [Test]
        public void CanCalculateStartAndEndpointOfTrendLine()
        {
            double startX = 1;
            double startY = 115.8119658;
            double endX = 13;
            double endY = 1806.837607;
            Assert.AreEqual(Math.Round(startX, 6), _target.Start.Key);
            Assert.AreEqual(Math.Round(startY, 6), Math.Round(_target.Start.Value, 6));
            Assert.AreEqual(Math.Round(endX, 6), _target.End.Key);
            Assert.AreEqual(Math.Round(endY, 6), Math.Round(_target.End.Value, 6));
        }
        [Test]
        public void CanHandleSingleDaySelection()
        {
            _target = new VolumeTrend(new List<double>{166.6666667d});
            Assert.AreEqual(1, _target.Start.Key);
            Assert.AreEqual(0, Math.Round(_target.Start.Value, 6));
            Assert.AreEqual(1, _target.End.Key);
            Assert.AreEqual(0, Math.Round(_target.End.Value, 6));
        }

        [Test]
        public void CanChangeTrendLine()
        {
	        var callCollection = Enumerable.Repeat(1d, 365).ToArray();
            double expectedresult = 1d*1.1;

			_target = new VolumeTrend(callCollection);
            _target.ChangeTrendLine(new Percent(0.1));

            Assert.AreEqual(Math.Round(expectedresult, 4), Math.Round(_target.End.Value, 4));
        }

        [Test]
        public void CanCalculateRakeInPercent()
        {
            Assert.AreEqual(409.965d,Math.Round(_target.Trend.Value,3));
        }

        [Test]
        public void CanGetDayChangeFactor()
        {
			var callCollection = Enumerable.Repeat(1d, 365).ToArray();

			_target = new VolumeTrend(callCollection);
            _target.ChangeTrendLine(new Percent(0));

            Assert.AreEqual(1, VolumeTrend.DayChangeFactor);
        }

        [Test]
        public void CanCalculateTrendFactorForStartDayInApplyPeriod()
        {
			var startTrendPeriod = new DateOnly(2007, 1, 1);
			var startForecastPeriod = new DateOnly(2007, 2, 1);
            double value = _trendDayFactor.Value * (1d / 365d);
            int diff = startForecastPeriod.Subtract(startTrendPeriod).Days;
            double expectedStartValue = (value * diff) + 1;
            double startDayFactor = VolumeTrend.CalculateStartDayFactor(startTrendPeriod, startForecastPeriod, _trendDayFactor);

            Assert.AreEqual(expectedStartValue, startDayFactor);

			var callCollection = Enumerable.Repeat(1d, 365).ToArray();
			_target = new VolumeTrend(callCollection);
            startDayFactor = VolumeTrend.CalculateStartDayFactor(startTrendPeriod, startTrendPeriod.AddDays(365), new Percent(1));

            Assert.AreEqual(2, startDayFactor);
        }
    }
}
