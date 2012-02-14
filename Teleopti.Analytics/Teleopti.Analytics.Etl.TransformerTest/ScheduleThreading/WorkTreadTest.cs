using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.ScheduleThreading
{
    [TestFixture]
    public class WorkTreadTest
    {
        private ThreadObj _threadObj1;
        private ThreadObj _threadObj2;

        [SetUp]
        public void Setup()
        {
            DateTime start = new DateTime(1990, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            DateTime end = new DateTime(2010, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            IJobParameters jobParameters = JobParametersFactory.SimpleParameters(false);

            IList<IScheduleDay> scheduleList = SchedulePartFactory.CreateSchedulePartCollection();
            IList<ScheduleProjection> scheduleProjectionServiceList = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(scheduleList);

            List<ScheduleProjection> scheduleProjections1 = new List<ScheduleProjection> { scheduleProjectionServiceList[0] };
            List<ScheduleProjection> scheduleProjections2 = new List<ScheduleProjection> { scheduleProjectionServiceList[8] };

            jobParameters.Helper = new JobHelper(new RaptorRepositoryStub(), null,null);

            _threadObj1 = new ThreadObj(scheduleProjections1, new DateTime(), jobParameters);
            _threadObj2 = new ThreadObj(scheduleProjections2, new DateTime(), jobParameters);

            MockRepository mockRepository = new MockRepository();
            IScheduleDataRowCollectionFactory scheduleDataRowCollectionFactory = mockRepository.StrictMock<IScheduleDataRowCollectionFactory>();

            using (mockRepository.Record())
            {
                Expect.On(scheduleDataRowCollectionFactory)
                    .Call(scheduleDataRowCollectionFactory.CreateScheduleDataRowCollection(null, null, null, new DateTimePeriod(), new DateTime(), 0))
                    .IgnoreArguments()
                    .Return(new List<DataRow>())
                    .Repeat.Any();
            }
            _threadObj1.ScheduleDataRowFactory = scheduleDataRowCollectionFactory;
        }

        [Test]
        public void Verify()
        {
            WorkThreadClass.WorkThread(_threadObj1);
            WorkThreadClass.WorkThread(_threadObj2);
        }
    }
}
