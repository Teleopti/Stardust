using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Interfaces.Domain;
using BusinessUnitFactory = Teleopti.Analytics.Etl.TransformerTest.FakeData.BusinessUnitFactory;

namespace Teleopti.Analytics.Etl.TransformerTest.ScheduleThreading
{
    [TestFixture]
    public class ScheduleDataRowFactoryTest
    {
        private IList<ISchedulePart> _scheduleParts;

        [SetUp]
        public void Setup()
        {
            IBusinessUnit businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("bu1");
            businessUnit.SetId(Guid.NewGuid());
            StateHolderHelper.ClearAndSetStateHolder(businessUnit);
            AuthorizationService.DefaultService = new AuthorizationServiceWithFullAccess();


            _scheduleParts = new ScheduleFactory().CreateShiftCollection();
            //IList<IVisualLayer> sss = 
            //GetIntervalLayers(new DateTimePeriod(), _scheduleParts);



        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "scheduleProjectionService"), Test, Ignore]
        public void Verify()
        {
            DataTable dt = ScheduleInfrastructure.CreateEmptyDataTable();
            var intervalBase = new IntervalBase(1, 1);

            IList<ScheduleProjection> scheduleProjectionServiceList = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(_scheduleParts);

            foreach (ScheduleProjection scheduleProjectionService in scheduleProjectionServiceList)
            {
                //IVisualLayerCollection visualLayerCollection = scheduleProjectionService.SchedulePartProjection.ProjectedLayerCollection.FilterLayers(new DateTimePeriod());


                var timeSpan1 = new TimeSpan(2, 1, 1, 1);
                var timeSpan2 = new TimeSpan(12, 1, 1, 1);
                var timePeriod = new TimePeriod(12, 1, 13, 1);


                //IVisualLayerCollection visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(timeSpan1, timeSpan2);
                IVisualLayerCollection visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(timeSpan1, timeSpan2, timePeriod);


                //visualLayerCollection

                foreach (IVisualLayer layer in visualLayerCollection)
                {
                    //DataRow dataRow = 
                    ScheduleDataRowFactory.CreateScheduleDataRow(dt, layer, scheduleProjectionService, intervalBase, DateTime.Now, 288);
                }
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Analytics.Etl.Transformer.ScheduleThreading.ScheduleDataRowCollection"), Test, Ignore("RK: for now")]
        public void VerifyB()
        {
            //DataTable dt = ScheduleInfrastructure.CreateEmptyDataTable();
            //IntervalBase intervalBase = new IntervalBase(1, 1);

            IList<ScheduleProjection> scheduleProjectionServiceList = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(_scheduleParts);

            foreach (ScheduleProjection scheduleProjectionService in scheduleProjectionServiceList)
            {
                //IVisualLayerCollection visualLayerCollection = scheduleProjectionService.SchedulePartProjection.ProjectedLayerCollection.FilterLayers(new DateTimePeriod());

                new ScheduleDataRowCollection(ScheduleInfrastructure.CreateEmptyDataTable(), scheduleProjectionService,
                                              new IntervalBase(1, 1), new DateTimePeriod(), DateTime.Now, 288);
            }
        }


    }
}
