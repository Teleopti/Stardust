using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.ScheduleThreading
{
    [TestFixture]
    public class DayAbsenceDataRowFactoryTest
    {
        private DataRow _dayAbsenceDataRow;
        private ScheduleProjection _scheduleProjection;
        private int _intervalsPerDay;

        private IList<IScheduleDay> _schedulePartCollection;

        [SetUp]
        public void Setup()
        {
            _intervalsPerDay = 96;
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
            
            Assert.AreEqual(absencePeriod.StartDateTime.Date, _dayAbsenceDataRow["date"]);
            Assert.AreEqual(new IntervalBase(absencePeriod.StartDateTime, _intervalsPerDay).Id,
                            _dayAbsenceDataRow["start_interval_id"]);
            Assert.AreEqual(_scheduleProjection.SchedulePart.Person.Id, _dayAbsenceDataRow["person_code"]);
            Assert.AreEqual(_scheduleProjection.SchedulePart.Scenario.Id, _dayAbsenceDataRow["scenario_code"]);
            Assert.AreEqual(absencePeriod.StartDateTime, _dayAbsenceDataRow["starttime"]);
            Assert.AreEqual(absence.Id, _dayAbsenceDataRow["day_absence_code"]);
            Assert.AreEqual(1, _dayAbsenceDataRow["day_count"]);
            Assert.AreEqual(absence.BusinessUnit.Id, _dayAbsenceDataRow["business_unit_code"]);
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(personAbsence),
                            _dayAbsenceDataRow["datasource_update_date"]);
        }
    }
}
