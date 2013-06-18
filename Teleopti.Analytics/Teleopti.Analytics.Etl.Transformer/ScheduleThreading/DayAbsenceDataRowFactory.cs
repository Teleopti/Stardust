using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
	public static class DayAbsenceDataRowFactory
	{
		public static DataRow CreateDayAbsenceDataRow(DataTable dataTable, ScheduleProjection scheduleProjection, int intervalsPerDay)
		{
			DataRow row = dataTable.NewRow();

			var columnArray = new object[12];
			IVisualLayer absenceLayer = scheduleProjection.SchedulePartProjection.First();
			IPersonAbsence personAbsence = GetPersonAbsenceForLayer(scheduleProjection.SchedulePart, absenceLayer);

			columnArray[0] = absenceLayer.Period.StartDateTime.Date;    // date
			columnArray[1] = new IntervalBase(absenceLayer.Period.StartDateTime, intervalsPerDay).Id;    // start_interval_id
			columnArray[2] = scheduleProjection.SchedulePart.Person.Id;     // person_code
			columnArray[3] = scheduleProjection.SchedulePart.Scenario.Id;     // scenario_code
			columnArray[4] = absenceLayer.Period.StartDateTime;     // starttime
			columnArray[5] = absenceLayer.Payload.Id.Value;     // day_absence_code
			columnArray[6] = 1;     // day_count
			columnArray[7] = absenceLayer.Payload.BusinessUnit.Id;     // business_unit_code
			// Leave datasource_id, insert_date and update_date since they have default values in db table
			columnArray[11] = RaptorTransformerHelper.GetUpdatedDate(personAbsence);     // datasource_update_date

			row.ItemArray = columnArray;

			return row;
		}

		public static IPersonAbsence GetPersonAbsenceForLayer(IScheduleDay schedule, ILayer<IPayload> layer)
		{
			IList<IPersonAbsence> pAbsenceCollection = schedule.PersonAbsenceCollection();
			return pAbsenceCollection.FirstOrDefault(personAbsence => personAbsence.Period.Contains(layer.Period));
		}
	}
}
