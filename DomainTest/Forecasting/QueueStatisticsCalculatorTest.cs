using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class QueueStatisticsCalculatorTest
    {
        private IStatisticTask _statistic;
        private QueueAdjustment _queueAdjustments;
        private IQueueStatisticsCalculator _target;

        [SetUp]
        public void Setup()
        {
            _statistic = new StatisticTask
                             {
                                 StatOfferedTasks = 1000,
                                 StatOverflowInTasks = 20,
                                 StatOverflowOutTasks = 30,
                                 StatAbandonedTasks = 100,
                                 StatAbandonedShortTasks = 20,
                                 StatAbandonedTasksWithinSL = 50
                             };
            _queueAdjustments = new QueueAdjustment();
        }

        [Test]
        public void VerifyCalculation()
        {
            _queueAdjustments.OfferedTasks = new Percent(0.8);
            _queueAdjustments.OverflowIn = new Percent(0.75);
            _queueAdjustments.OverflowOut = new Percent(-0.40);
            _queueAdjustments.Abandoned = new Percent(-1);
            _queueAdjustments.AbandonedShort = new Percent(0.2);
            _queueAdjustments.AbandonedWithinServiceLevel = new Percent(0.60);
            _queueAdjustments.AbandonedAfterServiceLevel = new Percent(0.5);
            _target = new QueueStatisticsCalculator(_queueAdjustments);

            _target.Calculate(_statistic);
            Assert.AreEqual(752d,_statistic.StatCalculatedTasks);
        }

		[Test]
		public void ShouldSetZeroWhenCalculationEndsInNegativeResult()
		{
			_queueAdjustments.OfferedTasks = new Percent(0.01);
			_queueAdjustments.OverflowIn = new Percent(0.75);
			_queueAdjustments.OverflowOut = new Percent(0.40);
			_queueAdjustments.Abandoned = new Percent(-1);
			_queueAdjustments.AbandonedShort = new Percent(0.2);
			_queueAdjustments.AbandonedWithinServiceLevel = new Percent(0.60);
			_queueAdjustments.AbandonedAfterServiceLevel = new Percent(0.5);
			_target = new QueueStatisticsCalculator(_queueAdjustments);

			_target.Calculate(_statistic);
			Assert.AreEqual(0d, _statistic.StatCalculatedTasks);
		}

        [Test]
        public void VerifyCalculationWithStatisticNull()
        {
            _queueAdjustments.OfferedTasks = new Percent(0.8);
            _queueAdjustments.OverflowIn = new Percent(0.75);
            _queueAdjustments.OverflowOut = new Percent(-0.40);
            _queueAdjustments.Abandoned = new Percent(-1);
            _queueAdjustments.AbandonedShort = new Percent(0.2);
            _queueAdjustments.AbandonedWithinServiceLevel = new Percent(0.60);
            _queueAdjustments.AbandonedAfterServiceLevel = new Percent(0.5);
            _target = new QueueStatisticsCalculator(_queueAdjustments);

            _statistic = null;
            _target.Calculate(_statistic);
            Assert.IsNull(_statistic);
        }
    }
}
