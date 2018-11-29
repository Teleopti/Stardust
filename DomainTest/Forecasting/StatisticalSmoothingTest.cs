using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class StatisticalSmoothingTest
    {
        private StatisticalSmoothing target;
        private StatisticalSmoothing targetUnsortedDictionary;
        private StatisticalSmoothing targetZeroValues;
        private IDictionary<DateTimePeriod, double> _numbers;
        private double _origSum;
        private DateTimePeriod _dateTimePeriod;

        [SetUp]
        public void Setup()
        {
            DateTime date = new DateTime(2008,3,20,8,0,0);
            DateTime date2 = new DateTime(2008, 3, 20, 8, 15, 0);

            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            date2 = DateTime.SpecifyKind(date2, DateTimeKind.Utc);

            _dateTimePeriod = new DateTimePeriod(date,date2);


            _numbers = new Dictionary<DateTimePeriod,double>();
            _numbers.Add(_dateTimePeriod,10);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0,15,0)), 30);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 30, 0)), 50);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 45, 0)), 20);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 60, 0)), 30);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 75, 0)), 100);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 90, 0)), 5);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 105, 0)), 65);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 120, 0)), 25);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 135, 0)), 30);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 150, 0)), 20);
            _origSum = 0;
            foreach (double number in _numbers.Values)
            {
                _origSum += number;
            }
            target = new StatisticalSmoothing(_numbers);
        }
        private static double ComputeSum(IEnumerable<double> values)
        {
            double newSum = 0;
            foreach (double value in values)
            {
                newSum += value;
            }
            return newSum;
        }



        [Test]
        public void CanCalculateRunningAverage3Periods()
        {
            //Target output for setup values with gliding 3 periods
            //(10+30)/2     = 20
            //(10+30+50)/3  = 30
            //(30+50+20)/3  = 33.33333333333333
            //(50+20+30)/3  = 33.33333333333333
            //(20+30+100)/3  = 50
            //(30+100+5)/3  = 45
            //(100+5+65)/3  = 56.6666666667
            //(5+65+25)/3  = 31.6666666667
            //(65+25+30)/3  = 40
            //(25+30+20)/3  = 25
            //(30+20)/2  = 25

            IList<double> testResultNumbers = new List<double>();
            testResultNumbers.Add(20);
            testResultNumbers.Add(30);
            testResultNumbers.Add(33.33333333333333);
            testResultNumbers.Add(33.33333333333333);
            testResultNumbers.Add(50);
            testResultNumbers.Add(45);
            testResultNumbers.Add(56.6666666667);
            testResultNumbers.Add(31.6666666667);
            testResultNumbers.Add(40);
            testResultNumbers.Add(25);
            testResultNumbers.Add(25);

            IDictionary<DateTimePeriod, double> glidingAverageNumbers = target.CalculateRunningAverage(3);
            double newSum = ComputeSum(testResultNumbers);
            double factor = _origSum/newSum;
            int index = 0;
            foreach (KeyValuePair<DateTimePeriod, double> pair in glidingAverageNumbers)
            {
                double modified = testResultNumbers[index] * factor;
                double expected = Math.Round(modified, 2);
                double actual = Math.Round(pair.Value, 2);
                Assert.AreEqual(expected, actual);
                index++;
            }
        }

        [Test]
        public void CanCalculateRunningAverage5Periods()
        {
            //Target output for setup values with gliding 5 periods
            //(10+30+50)/3     = 30
            //(10+30+50+20)/4  = 27.5
            //(10+30+50+20+30)/5  = 28
            //(30+50+20+30+100)/5  = 46
            //(50+20+30+100+5)/5  = 41
            //(20+30+100+5+65)/5  = 44
            //(30+100+5+65+25)/5  = 45
            //(100+5+65+25+30)/5  = 45
            //(5+65+25+30+20)/5  = 29
            //(65+25+30+20)/4  = 35
            //(25+30+20)/3  = 25

            IList<double> testResultNumbers = new List<double>();
            testResultNumbers.Add(30);
            testResultNumbers.Add(27.5);
            testResultNumbers.Add(28);
            testResultNumbers.Add(46);
            testResultNumbers.Add(41);
            testResultNumbers.Add(44);
            testResultNumbers.Add(45);
            testResultNumbers.Add(45);
            testResultNumbers.Add(29);
            testResultNumbers.Add(35);
            testResultNumbers.Add(25);

            IDictionary<DateTimePeriod, double> glidingAverageNumbers = target.CalculateRunningAverage(5); 
            double newSum = ComputeSum(testResultNumbers);
            double factor = _origSum / newSum;
            int index = 0;
            foreach (KeyValuePair<DateTimePeriod, double> pair in glidingAverageNumbers)
            {
                double modified = testResultNumbers[index] * factor;
                double expected = Math.Round(modified, 2);
                double actual = Math.Round(pair.Value, 2);
                Assert.AreEqual(expected, actual);
                index++;
            }
        }

        [Test]
        public void VerifySumIsSameAfterSmoothening()
        {
            double expectedSum = 0;
            double testSum = 0;
            foreach (KeyValuePair<DateTimePeriod, double> valuePair in _numbers)
            {
                expectedSum += valuePair.Value;
            }
            
            IDictionary<DateTimePeriod, double> glidingAverageNumbers = target.CalculateRunningAverage(7);
            foreach (KeyValuePair<DateTimePeriod, double> valuePair in glidingAverageNumbers)
            {
                testSum += valuePair.Value;
            }

            Assert.AreEqual(expectedSum, testSum);
        }

        [Test]
        public void CanCalculateRunningAverage7Periods()
        {
            //Target output for setup values with gliding 7 periods
           
            //(10+30+50+20)/4  = 27.5
            //(10+30+50+20+30)/5  = 28
            //(10+30+50+20+30+100)/6  = 40
            //(10+30+50+20+30+100+5)/7  = 35
            //(30+50+20+30+100+5+65)/7  = 42.85714285714286
            //(50+20+30+100+5+65+25)/7  = 42.14285714285714
            //(20+30+100+5+65+25+30)/7  = 39.28571428571429
            //(30+100+5+65+25+30+20)/7  = 39.28571428571429
            //(100+5+65+25+30+20)/6  = 40.83333333333
            //(5+65+25+30+20)/5  = 29
            //(65+25+30+20)/4  = 35

            IList<double> testResultNumbers = new List<double>();
            testResultNumbers.Add(27.5);
            testResultNumbers.Add(28);
            testResultNumbers.Add(40);
            testResultNumbers.Add(35);
            testResultNumbers.Add(42.85714285714286);
            testResultNumbers.Add(42.14285714285714);
            testResultNumbers.Add(39.28571428571429);
            testResultNumbers.Add(39.28571428571429);
            testResultNumbers.Add(40.833333333333);
            testResultNumbers.Add(29);
            testResultNumbers.Add(35);

            IDictionary<DateTimePeriod, double> runningAverageNumbers = target.CalculateRunningAverage(7);
            double newSum = ComputeSum(testResultNumbers);
            double factor = _origSum / newSum;
            int index = 0;
            foreach (KeyValuePair<DateTimePeriod, double> pair in runningAverageNumbers)
            {
                double modified = testResultNumbers[index] * factor;
                double expected = Math.Round(modified, 2);
                double actual = Math.Round(pair.Value, 2);
                Assert.AreEqual(expected, actual);
                index++;
            }
        }

        [Test]
        public void canHandleRunningAverageWithInsuffientIntervals()
        {

            DateTime date = new DateTime(2008, 3, 20, 8, 0, 0);
            DateTime date2 = new DateTime(2008, 3, 20, 8, 15, 0);

            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            date2 = DateTime.SpecifyKind(date2, DateTimeKind.Utc);

            _dateTimePeriod = new DateTimePeriod(date, date2);

            _numbers = new Dictionary<DateTimePeriod, double>();
            _numbers.Add(_dateTimePeriod, 10);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 15, 0)), 30);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 30, 0)), 50);
            _numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 45, 0)), 20);

            _origSum = 0;
            foreach (double number in _numbers.Values)
            {
                _origSum += number;
            }
            target = new StatisticalSmoothing(_numbers);

            //Target output for setup values with gliding 7 periods
            //No change because of less no of intervals(4) than smoothing factor(7)

            IList<double> testResultNumbers = new List<double>();
            testResultNumbers.Add(10);
            testResultNumbers.Add(30);
            testResultNumbers.Add(50);
            testResultNumbers.Add(20);

            IDictionary<DateTimePeriod, double> runningAverageNumbers = target.CalculateRunningAverage(7);
            double newSum = ComputeSum(testResultNumbers);
            double factor = _origSum / newSum;
            int index = 0;
            foreach (KeyValuePair<DateTimePeriod, double> pair in runningAverageNumbers)
            {
                double modified = testResultNumbers[index] * factor;
                double expected = Math.Round(modified, 2);
                double actual = Math.Round(pair.Value, 2);
                Assert.AreEqual(expected, actual);
                index++;
            }
        }

        [Test]
        public void CanHandleZeroValues()
        {
            DateTime date = new DateTime(2008, 3, 20, 8, 0, 0);
            DateTime date2 = new DateTime(2008, 3, 20, 8, 15, 0);

            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            date2 = DateTime.SpecifyKind(date2, DateTimeKind.Utc);

            DateTimePeriod dateTimePeriod = new DateTimePeriod(date, date2);

            IDictionary<DateTimePeriod, double> numbers = new Dictionary<DateTimePeriod, double>();
            numbers.Add(dateTimePeriod, 0);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 15, 0)), 0);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 30, 0)), 0);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 45, 0)), 0);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 60, 0)), 0);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 75, 0)), 100);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 90, 0)), 5);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 105, 0)), 65);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 120, 0)), 25);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 135, 0)), 30);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 150, 0)), 20);

            targetZeroValues = new StatisticalSmoothing(numbers);
            //Target output for setup values with gliding 3 periods
            //(0+0)/2    = 0
            //(0+0+0)/3  = 0
            //(0+0+0)/3  = 0
            //(0+0+0)/3  = 0
            //(0+0+100)/3  = 33.33333333333333
            //(0+100+5)/3  = 35
            //(100+5+65)/3  = 56.66666666666667
            //(5+65+25)/3  = 31.66666666666667
            //(65+25+30)/3  = 40
            //(25+30+20)/3  = 25
            //(30+20)/2  = 25

            IList<double> testResultNumbers = new List<double>();
            testResultNumbers.Add(0);
            testResultNumbers.Add(0);
            testResultNumbers.Add(0);
            testResultNumbers.Add(0);
            testResultNumbers.Add(33.33333333333333);
            testResultNumbers.Add(35);
            testResultNumbers.Add(56.66666666666667);
            testResultNumbers.Add(31.66666666666667);
            testResultNumbers.Add(40);
            testResultNumbers.Add(25);
            testResultNumbers.Add(25);
            double origSum = 0;
            foreach (double number in numbers.Values)
            {
                origSum += number;
            }
            IDictionary<DateTimePeriod, double> runningAverageNumbers = targetZeroValues.CalculateRunningAverage(3);
            double newSum = ComputeSum(testResultNumbers);
            double factor = origSum / newSum;
            int index = 0;
            foreach (KeyValuePair<DateTimePeriod, double> pair in runningAverageNumbers)
            {
                double modified = testResultNumbers[index] * factor;
                double expected = Math.Round(modified, 2);
                double actual = Math.Round(pair.Value, 2);
                Assert.AreEqual(expected, actual);
                index++;
            }
        }

        [Test]
        public void CanHandleUnsortedDictionary()
        {
            DateTime date = new DateTime(2008, 3, 20, 8, 0, 0);
            DateTime date2 = new DateTime(2008, 3, 20, 8, 15, 0);

            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            date2 = DateTime.SpecifyKind(date2, DateTimeKind.Utc);

            DateTimePeriod dateTimePeriod = new DateTimePeriod(date, date2);

            IDictionary<DateTimePeriod, double> numbers = new Dictionary<DateTimePeriod, double>();
            
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 15, 0)), 30);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 30, 0)), 50);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 45, 0)), 20);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 60, 0)), 30);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 75, 0)), 100);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 90, 0)), 5);
            numbers.Add(dateTimePeriod, 10);  //Unsorted  !!!
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 105, 0)), 65);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 120, 0)), 25);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 135, 0)), 30);
            numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 150, 0)), 20);

            targetUnsortedDictionary = new StatisticalSmoothing(numbers);
            //Target output for setup values with gliding 3 periods
            //(10+30)/2     = 20
            //(10+30+50)/3  = 30
            //(30+50+20)/3  = 33.33
            //(50+20+30)/3  = 33.33
            //(20+30+100)/3  = 50
            //(30+100+5)/3  = 45
            //(100+5+65)/3  = 56.67
            //(5+65+25)/3  = 31.67
            //(65+25+30)/3  = 40
            //(25+30+20)/3  = 25
            //(30+20)/2  = 25

            IList<double> testResultNumbers = new List<double>();
            testResultNumbers.Add(20);
            testResultNumbers.Add(30);
            testResultNumbers.Add(33.3333333);
            testResultNumbers.Add(33.3333333);
            testResultNumbers.Add(50);
            testResultNumbers.Add(45);
            testResultNumbers.Add(56.66666667);
            testResultNumbers.Add(31.66666667);
            testResultNumbers.Add(40);
            testResultNumbers.Add(25);
            testResultNumbers.Add(25);
            double origSum = 0;
            foreach (double number in numbers.Values)
            {
                origSum += number;
            }

            IDictionary<DateTimePeriod, double> runningAverageNumbers = targetUnsortedDictionary.CalculateRunningAverage(3);
            double newSum = ComputeSum(testResultNumbers);
            double factor = origSum / newSum;
            int index = 0;
            foreach (KeyValuePair<DateTimePeriod, double> pair in runningAverageNumbers)
            {
                double modified = testResultNumbers[index] * factor;
                double expected = Math.Round(modified, 2);
                double actual = Math.Round(pair.Value, 2);
                Assert.AreEqual(expected, actual);
                index++;
            }
        }

        [Test]
        public void VerifyExceptionIsThrownIfSmootheningFactorIsBelow1()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => target.CalculateRunningAverage(0));
        }


        /// <summary>
        /// Verifies the smoothing when all numbers are zero.
        /// Bugg: 12603
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2010-11-29
        /// </remarks>
        [Test]
        public void VerifySmoothingWhenAllNumbersAreZero()
        {

            IDictionary<DateTimePeriod, double> numbers = new Dictionary<DateTimePeriod, double>();
            numbers.Add(_dateTimePeriod, 0);
            numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 15, 0)), 0);
            numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 30, 0)), 0);
            numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 45, 0)), 0);
            numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 60, 0)), 0);
            numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 75, 0)), 0);
            numbers.Add(_dateTimePeriod.MovePeriod(new TimeSpan(0, 90, 0)), 0);
            target = new StatisticalSmoothing(numbers);

            IDictionary<DateTimePeriod, double> glidingAverageNumbers = target.CalculateRunningAverage(3);


            foreach (KeyValuePair<DateTimePeriod, double> glidingAverageNumber in glidingAverageNumbers)
            {
                Assert.IsFalse(double.IsNaN(glidingAverageNumber.Value));
                break;
            }
        }

		[Test]
		public void CanHandleSameValues()
		{
			DateTime date = new DateTime(2008, 3, 20, 8, 0, 0);
			DateTime date2 = new DateTime(2008, 3, 20, 8, 15, 0);

			date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
			date2 = DateTime.SpecifyKind(date2, DateTimeKind.Utc);

			DateTimePeriod dateTimePeriod = new DateTimePeriod(date, date2);

			IDictionary<DateTimePeriod, double> numbers = new Dictionary<DateTimePeriod, double>();
			numbers.Add(dateTimePeriod, 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 15, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 30, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 45, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 60, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 75, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 90, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 105, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 120, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 135, 0)), 25);
			numbers.Add(dateTimePeriod.MovePeriod(new TimeSpan(0, 150, 0)), 25);

			targetZeroValues = new StatisticalSmoothing(numbers);

			IDictionary<DateTimePeriod, double> runningAverageNumbers = targetZeroValues.CalculateRunningAverage(3);
			int index = 0;
			foreach (KeyValuePair<DateTimePeriod, double> pair in runningAverageNumbers)
			{
				double expected = 25;
				double actual = Math.Round(pair.Value, 2);
				Assert.AreEqual(expected, actual);
				index++;
			}
		}
    }
}
