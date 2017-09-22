using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class TempTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
        }

        [Test]
        public void Test()
        {
            var scheduleFactory = new ScheduleFactory();
            IList<IScheduleDay> schedulePartList = scheduleFactory.CreateShiftCollection();
            IList<ScheduleProjection> scheduleProjectionServiceList =
                ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(schedulePartList);

            ThreadObjects.GetThreadObjectsSplitByPeriod(scheduleProjectionServiceList, DateTime.Now, 1, _jobParameters);
        }
    }
}
