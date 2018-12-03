using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Entity
{
    [TestFixture]
    public class EtlJobRelativePeriodTest
    {
        private IEtlJobRelativePeriod _target;
        MinMax<int> _minMaxRelativePeriod = new MinMax<int>(-7, 7);

        [Test]
        public void VerifyCanCreateAndPropertiesWork()
        {
            _target = new EtlJobRelativePeriod(_minMaxRelativePeriod, JobCategoryType.Schedule);

            Assert.AreEqual(_minMaxRelativePeriod.Minimum, _target.RelativePeriod.Minimum);
            Assert.AreEqual(_minMaxRelativePeriod.Maximum, _target.RelativePeriod.Maximum);
            Assert.AreEqual(JobCategoryType.Schedule, _target.JobCategory);
        }

        [Test]
        public void VerifyJobCategoryName()
        {
            IEtlJobRelativePeriod relativePeriodInitial = new EtlJobRelativePeriod(_minMaxRelativePeriod,
                                                                            JobCategoryType.Initial);
            IEtlJobRelativePeriod relativePeriodAgentStats = new EtlJobRelativePeriod(_minMaxRelativePeriod,
                                                                            JobCategoryType.AgentStatistics);
            IEtlJobRelativePeriod relativePeriodQueueStats = new EtlJobRelativePeriod(_minMaxRelativePeriod,
                                                                            JobCategoryType.QueueStatistics);
            IEtlJobRelativePeriod relativePeriodSchedule = new EtlJobRelativePeriod(_minMaxRelativePeriod,
                                                                            JobCategoryType.Schedule);
            IEtlJobRelativePeriod relativePeriodForecast = new EtlJobRelativePeriod(_minMaxRelativePeriod,
                                                                            JobCategoryType.Forecast);

            Assert.AreEqual("Initial", relativePeriodInitial.JobCategoryName);
            Assert.AreEqual("Agent Statistics", relativePeriodAgentStats.JobCategoryName);
            Assert.AreEqual("Queue Statistics", relativePeriodQueueStats.JobCategoryName);
            Assert.AreEqual("Schedule", relativePeriodSchedule.JobCategoryName);
            Assert.AreEqual("Forecast", relativePeriodForecast.JobCategoryName);
        }
    }
}
