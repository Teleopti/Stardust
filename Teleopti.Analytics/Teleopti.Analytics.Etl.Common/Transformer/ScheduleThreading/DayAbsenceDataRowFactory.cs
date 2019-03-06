using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public static class DayAbsenceDataRowFactory
	{
		public static DataRow CreateDayAbsenceDataRow(DataTable dataTable, ScheduleProjection scheduleProjection)
		{
			DataRow row = dataTable.NewRow();

			var columnArray = new object[8];
			IVisualLayer absenceLayer = scheduleProjection.SchedulePartProjection.First();
			IPersonAbsence personAbsence = GetPersonAbsenceForLayer(scheduleProjection.SchedulePart, absenceLayer);

			row["schedule_date_local"] = scheduleProjection.SchedulePart.DateOnlyAsPeriod.DateOnly.Date;
			row["person_code"] = scheduleProjection.SchedulePart.Person.Id;
			row["scenario_code"] = scheduleProjection.SchedulePart.Scenario.Id;
			row["starttime"] = absenceLayer.Period.StartDateTime;
			row["absence_code"] = absenceLayer.Payload.Id.Value;
			row["day_count"] = 1;
			row["business_unit_code"] = absenceLayer.Payload.GetOrFillWithBusinessUnit_DONTUSE().Id;
			row["datasource_id"] = 1;
			row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(personAbsence);

			return row;
		}

		public static IPersonAbsence GetPersonAbsenceForLayer(IScheduleDay schedule, ILayer<IPayload> layer)
		{
			IList<IPersonAbsence> pAbsenceCollection = schedule.PersonAbsenceCollection();
			return pAbsenceCollection.FirstOrDefault(personAbsence => personAbsence.Period.Contains(layer.Period));
		}
	}
}
