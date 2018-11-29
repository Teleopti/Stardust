using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class QueueStatisticsProviderTest
    {
        private IList<IStatisticTask> _statisticsList;
        private QueueStatisticsProvider _target;
        private IQueueStatisticsCalculator _calculator;
        private DateTime _dateTime;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _dateTime = new DateTime(2010, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            _statisticsList = new List<IStatisticTask>();
            _calculator = _mocks.StrictMock<IQueueStatisticsCalculator>();
            _target = new QueueStatisticsProvider(_statisticsList,_calculator);
        }

        [Test]
        public void VerifyCanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanSumValuesWhenTwoTasksAreOnSameInterval()
        {
            using (_mocks.Record())
            {
                Expect.Call(()=>_calculator.Calculate(null)).IgnoreArguments();
            }

            _statisticsList.Add(new StatisticTask
                                    {
                                        Interval = _dateTime,
                                        StatAnsweredTasks = 200,
                                        StatAverageTaskTimeSeconds = 20,
                                        StatAverageAfterTaskTimeSeconds = 40
                                    });
            _statisticsList.Add(new StatisticTask
                                    {
                                        Interval = _dateTime,
                                        StatAnsweredTasks = 300,
                                        StatAverageTaskTimeSeconds = 30,
                                        StatAverageAfterTaskTimeSeconds = 60
                                    });

            using (_mocks.Playback())
            {
                _target = new QueueStatisticsProvider(_statisticsList,_calculator);
                var result = _target.GetStatisticsForPeriod(new DateTimePeriod(_dateTime, _dateTime.AddMinutes(15)));

                Assert.AreEqual(500, result.StatAnsweredTasks);
                Assert.AreEqual(26, result.StatAverageTaskTimeSeconds);
                Assert.AreEqual(52, result.StatAverageAfterTaskTimeSeconds);
            }
        }

        [Test]
        public void VerifyCanSumAbandonedValuesWhenTwoTasksAreOnSameInterval()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _calculator.Calculate(null)).IgnoreArguments();
            }

            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime,
                StatAbandonedShortTasks = 2,
                StatAbandonedTasksWithinSL = 3
            });
            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime,
                StatAbandonedShortTasks = 3,
                StatAbandonedTasksWithinSL = 4
            });

            using (_mocks.Playback())
            {
                _target = new QueueStatisticsProvider(_statisticsList, _calculator);
                var result = _target.GetStatisticsForPeriod(new DateTimePeriod(_dateTime, _dateTime.AddMinutes(15)));

                Assert.AreEqual(5, result.StatAbandonedShortTasks);
                Assert.AreEqual(7, result.StatAbandonedTasksWithinSL);
            }
        }

        [Test]
        public void VerifyCanSumValuesWhenTwoTasksAreOnIncludedInterval()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _calculator.Calculate(null)).IgnoreArguments();
            }
            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime,
                StatAnsweredTasks = 200,
                StatAverageTaskTimeSeconds = 20,
                StatAverageAfterTaskTimeSeconds = 40
            });
            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime.AddMinutes(15),
                StatAnsweredTasks = 300,
                StatAverageTaskTimeSeconds = 30,
                StatAverageAfterTaskTimeSeconds = 60
            });

            using (_mocks.Playback())
            {
                _target = new QueueStatisticsProvider(_statisticsList,_calculator);

                var result = _target.GetStatisticsForPeriod(new DateTimePeriod(_dateTime, _dateTime.AddMinutes(30)));
                Assert.AreEqual(500, result.StatAnsweredTasks);
                Assert.AreEqual(26, result.StatAverageTaskTimeSeconds);
                Assert.AreEqual(52, result.StatAverageAfterTaskTimeSeconds);
            }
        }

        [Test]
        public void VerifyCanSumValuesWhenThreeStatisticTasksInsidePeriod()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _calculator.Calculate(null)).IgnoreArguments();
            }
            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime,
                StatAnsweredTasks = 200,
                StatAverageTaskTimeSeconds = 20,
                StatAverageAfterTaskTimeSeconds = 40
            });
            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime.AddMinutes(5),
                StatAnsweredTasks = 300,
                StatAverageTaskTimeSeconds = 30,
                StatAverageAfterTaskTimeSeconds = 60
            });

            _statisticsList.Add(new StatisticTask
            {
                Interval = _dateTime.AddMinutes(10),
                StatAnsweredTasks = 400,
                StatAverageTaskTimeSeconds = 40,
                StatAverageAfterTaskTimeSeconds = 80
            });

            using (_mocks.Playback())
            {
                _target = new QueueStatisticsProvider(_statisticsList,_calculator);

                var result = _target.GetStatisticsForPeriod(new DateTimePeriod(_dateTime, _dateTime.AddMinutes(15)));
                Assert.AreEqual(900, result.StatAnsweredTasks);
                Assert.AreEqual(32.22D, Math.Round(result.StatAverageTaskTimeSeconds, 2));
                Assert.AreEqual(64.44D, Math.Round(result.StatAverageAfterTaskTimeSeconds, 2));
            }
        }

        [Test]
        public void VerifyCanReturnEmptyStatisticsIfNoPeriodMatches()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _calculator.Calculate(null)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                var result = _target.GetStatisticsForPeriod(new DateTimePeriod(_dateTime, _dateTime.AddMinutes(15)));
                Assert.AreEqual(_dateTime, result.Interval);
                Assert.AreEqual(0, result.StatAnsweredTasks);
                Assert.AreEqual(0, result.StatAverageTaskTimeSeconds);
                Assert.AreEqual(0, result.StatAverageAfterTaskTimeSeconds);
            }
        }
    }
}
