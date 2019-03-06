using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
    [TestFixture]
    public class DayAbsenceDataRowFactoryTest
    {
        private DataRow _dayAbsenceDataRow;
        private ScheduleProjection _scheduleProjection;
        private IList<IScheduleDay> _schedulePartCollection;

        [SetUp]
        public void Setup()
        {
            _schedulePartCollection = SchedulePartFactory.CreateSchedulePartCollection();
            IList<ScheduleProjection> scheduleProjectionCollection = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(_schedulePartCollection);
            // Which object in scheduleProjectionCollection represents an whole day absence? Answer: index=8
            _scheduleProjection = scheduleProjectionCollection[8];

            using (DataTable dataTable = new DataTable())
            {
                dataTable.Locale = Thread.CurrentThread.CurrentCulture;
                ScheduleDayAbsenceCountInfrastructure.AddColumnsToDataTable(dataTable);
                _dayAbsenceDataRow = DayAbsenceDataRowFactory.CreateDayAbsenceDataRow(dataTable, _scheduleProjection);
            }
        }

        [Test]
        public void VerifyDataRowCreation()
        {
            DateTimePeriod absencePeriod =
                _scheduleProjection.SchedulePartProjection.Period().Value;
            IVisualLayer absenceLayer = _scheduleProjection.SchedulePartProjection.First();
            var absence = (IAbsence)absenceLayer.Payload;
            IPersonAbsence personAbsence = DayAbsenceDataRowFactory.GetPersonAbsenceForLayer(_scheduleProjection.SchedulePart, absenceLayer);

			Assert.AreEqual(absencePeriod.StartDateTime.Date, _dayAbsenceDataRow["schedule_date_local"]);
            Assert.AreEqual(_scheduleProjection.SchedulePart.Person.Id, _dayAbsenceDataRow["person_code"]);
            Assert.AreEqual(_scheduleProjection.SchedulePart.Scenario.Id, _dayAbsenceDataRow["scenario_code"]);
            Assert.AreEqual(absencePeriod.StartDateTime, _dayAbsenceDataRow["starttime"]);
            Assert.AreEqual(absence.Id, _dayAbsenceDataRow["absence_code"]);
            Assert.AreEqual(1, _dayAbsenceDataRow["day_count"]);
            Assert.AreEqual(absence.GetOrFillWithBusinessUnit_DONTUSE().Id, _dayAbsenceDataRow["business_unit_code"]);
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(personAbsence),
                            _dayAbsenceDataRow["datasource_update_date"]);
        }
    }
}
