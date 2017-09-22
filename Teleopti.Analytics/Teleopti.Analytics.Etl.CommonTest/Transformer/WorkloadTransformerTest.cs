using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class WorkloadTransformerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _workloadList = WorkloadFactory.CreateWorkloadList();
            _target = new WorkloadTransformer(_insertDateTime);

            using (var workloadTable = new DataTable())
            {
                workloadTable.Locale = Thread.CurrentThread.CurrentCulture;
                WorkloadInfrastructure.AddColumnsToDataTable(workloadTable);
                
                using (var workloadQueueTable = new DataTable())
                {
                    workloadQueueTable.Locale = Thread.CurrentThread.CurrentCulture;
                    WorkloadQueueInfrastructure.AddColumnsToDataTable(workloadQueueTable);

                    _target.Transform(_workloadList, workloadTable, workloadQueueTable);

                    Assert.AreEqual(3, workloadTable.Rows.Count);
                    _rowWorkload0 = workloadTable.Rows[0];
                    _rowWorkload1 = workloadTable.Rows[1];
                    _rowWorkload2 = workloadTable.Rows[2];

                    _rowWorkloadQueue0 = workloadQueueTable.Rows[0];
                    _rowWorkloadQueue1 = workloadQueueTable.Rows[1];
                    _rowWorkloadQueue2 = workloadQueueTable.Rows[2];
                }
            }
        }

        #endregion

        private IList<IWorkload> _workloadList;
        private WorkloadTransformer _target;
        private readonly DateTime _insertDateTime = DateTime.Now;
        private DataRow _rowWorkload0;
        private DataRow _rowWorkload1;
        private DataRow _rowWorkload2;
        private DataRow _rowWorkloadQueue0;
        private DataRow _rowWorkloadQueue1;
        private DataRow _rowWorkloadQueue2;

        [Test]
        public void VerifyTheMatrixInternalData()
        {
            Assert.AreEqual(1, _rowWorkload0["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _rowWorkload1["insert_date"]);
            Assert.AreEqual(_insertDateTime, _rowWorkload2["update_date"]);
        }

        [Test]
        public void VerifyAggregateRoot()
        {
            //BusinessUnit
            Assert.AreEqual(_workloadList[0].BusinessUnit.Id, _rowWorkload0["business_unit_code"]);
            Assert.AreEqual(_workloadList[1].BusinessUnit.Description.Name, _rowWorkload1["business_unit_name"]);
            //UpdatedOn
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_workloadList[0]),
                            _rowWorkload0["datasource_update_date"]);
        }

        [Test]
        public void VerifyIsDeleted()
        {
            var workloadNotDeleted = _workloadList[0] as IDeleteTag;
            var skillNotDeleted = _workloadList[0].Skill as IDeleteTag;
            var workloadDeleted = _workloadList[1] as IDeleteTag;
            var skillDeleted = _workloadList[1].Skill as IDeleteTag;

            Assert.IsNotNull(workloadNotDeleted);
            Assert.IsNotNull(skillNotDeleted);
            Assert.IsNotNull(workloadDeleted);
            Assert.IsNotNull(skillDeleted);
            Assert.IsFalse(workloadNotDeleted.IsDeleted);
            Assert.IsFalse(skillNotDeleted.IsDeleted);
            Assert.IsTrue(workloadDeleted.IsDeleted);
            Assert.IsTrue(skillDeleted.IsDeleted);

            Assert.AreEqual(workloadNotDeleted.IsDeleted, _rowWorkload0["is_deleted"]);
            Assert.AreEqual(skillNotDeleted.IsDeleted, _rowWorkload0["skill_is_deleted"]);
            Assert.AreEqual(workloadDeleted.IsDeleted, _rowWorkload1["is_deleted"]);
            Assert.AreEqual(skillDeleted.IsDeleted, _rowWorkload1["skill_is_deleted"]);
        }

        [Test]
        public void VerifyWorkload()
        {
            
            Assert.AreEqual(_workloadList[0].Id, _rowWorkload0["workload_code"]);
            Assert.AreEqual(_workloadList[2].Name, _rowWorkload2["workload_name"]);
            Assert.AreEqual(_workloadList[0].Skill.Id, _rowWorkload0["skill_code"]);
            Assert.AreEqual(_workloadList[1].Skill.Name, _rowWorkload1["skill_name"]);
            Assert.AreEqual(((Skill)_workloadList[0].Skill).TimeZone.Id, _rowWorkload0["time_zone_code"]);
            Assert.AreEqual(_workloadList[0].Skill.SkillType.Id, _rowWorkload0["forecast_method_code"]);
            Assert.AreEqual(_workloadList[2].Skill.SkillType.ForecastSource.ToString(), _rowWorkload2["forecast_method_name"]);
        }

        [Test]
        public void VerifyQueueAdjustments()
        {
            Assert.AreEqual(_workloadList[0].QueueAdjustments.OfferedTasks.Value, _rowWorkload0["percentage_offered"]);
            Assert.AreEqual(_workloadList[0].QueueAdjustments.OverflowIn.Value, _rowWorkload0["percentage_overflow_in"]);
            Assert.AreEqual(_workloadList[0].QueueAdjustments.OverflowOut.Value, _rowWorkload0["percentage_overflow_out"]);
            Assert.AreEqual(_workloadList[0].QueueAdjustments.Abandoned.Value, _rowWorkload0["percentage_abandoned"]);
            Assert.AreEqual(_workloadList[0].QueueAdjustments.AbandonedShort.Value, _rowWorkload0["percentage_abandoned_short"]);
            Assert.AreEqual(_workloadList[0].QueueAdjustments.AbandonedWithinServiceLevel.Value, _rowWorkload0["percentage_abandoned_within_service_level"]);
            Assert.AreEqual(_workloadList[0].QueueAdjustments.AbandonedAfterServiceLevel.Value, _rowWorkload0["percentage_abandoned_after_service_level"]);
        }

        [Test]
        public void VerifyWorkloadQueue()
        {
            Assert.AreEqual(1,
                           _rowWorkloadQueue0["datasource_id"]);

            Assert.AreEqual(1,
                           _rowWorkloadQueue1["datasource_id"]);

            Assert.AreEqual(1,
                           _rowWorkloadQueue2["datasource_id"]);

            Assert.AreEqual(_workloadList[0].Id,
                           _rowWorkloadQueue0["workload_code"]);

            Assert.AreEqual(_workloadList[1].Id,
                           _rowWorkloadQueue1["workload_code"]);

            Assert.AreEqual(_workloadList[2].Id,
                          _rowWorkloadQueue2["workload_code"]);
            //BusinessUnit
            Assert.AreEqual(_workloadList[0].BusinessUnit.Id, _rowWorkloadQueue0["business_unit_code"]);
            Assert.AreEqual(_workloadList[1].BusinessUnit.Description.Name, _rowWorkloadQueue0["business_unit_name"]);
        }

    }
}


