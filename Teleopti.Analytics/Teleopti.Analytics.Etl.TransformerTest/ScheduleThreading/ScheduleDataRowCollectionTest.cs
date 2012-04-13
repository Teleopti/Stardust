using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.ScheduleThreading
{
    [TestFixture]
    public class ScheduleDataRowCollectionTest
    {
        private IJobParameters _jobParameters;

        [SetUp]
        public void Setup()
        {
            _jobParameters = JobParametersFactory.SimpleParameters(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void Verify()
        {
            IList<IScheduleDay> scheduleList = SchedulePartFactory.CreateSchedulePartCollection();
            IList<ScheduleProjection> scheduleProjectionServiceList = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(scheduleList);

            List<ScheduleProjection> scheduleProjections = new List<ScheduleProjection> { scheduleProjectionServiceList[0] };

            _jobParameters.Helper = new JobHelper(new RaptorRepositoryStub(), null,null);
            
            IVisualLayerCollection layerCollection = VisualLayerCollectionFactory.CreateForWorkShift(new Person(), new TimeSpan(9, 0, 0), new TimeSpan(16, 0, 0),
                                                                new TimePeriod(11, 0, 12, 0));
            IVisualLayer layer = layerCollection.First();

            IntervalBase intervalBase = new IntervalBase(new DateTime(), 288);

            using (DataTable table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                ScheduleInfrastructure.AddColumnsToDataTable(table);
                ScheduleDataRowFactory.CreateScheduleDataRow(table, layer, scheduleProjections[0], intervalBase, DateTime.Now, 288);
            }
        }
    }
}
