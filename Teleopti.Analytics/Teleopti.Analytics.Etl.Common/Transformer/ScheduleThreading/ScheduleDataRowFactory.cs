using System;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public class ScheduleDataRowFactory
	{
		public DataRow CreateScheduleDataRow(DataTable dataTable
			,IVisualLayer layer
			,IScheduleProjection scheduleProjection
			,IntervalBase interval
			,DateTime insertDateTime
			,int intervalsPerDay
			,IVisualLayerCollection layerCollection
			,DateTime shiftStart
			,DateTime shiftEnd)
		{
			var nullGuid = Guid.Empty;

			IPayload payload = layer.Payload;
			DataRow row = dataTable.NewRow();

			row["schedule_date_local"] = scheduleProjection.SchedulePart.DateOnlyAsPeriod.DateOnly.Date;
			row["schedule_date_utc"] = layer.Period.StartDateTime.Date;
			row["person_code"] = scheduleProjection.SchedulePart.Person.Id;
			row["interval_id"] = interval.Id;
			row["activity_start"] = layer.Period.StartDateTime;
			row["scenario_code"] = scheduleProjection.SchedulePart.Scenario.Id;

			row["activity_code"] = nullGuid;
			row["absence_code"] = nullGuid;

			var meetingPayload = payload as IMeetingPayload;
			var activity = payload as IActivity;
			var absence = payload as IAbsence;
			if (meetingPayload != null)
			{
				activity = meetingPayload.Meeting.Activity;
			}
			if (absence != null)
			{
				row["absence_code"] = absence.Id;
				row["shift_category_code"] = nullGuid;
			}

			if (activity != null)
			{
				IPersonAssignment personAssignment =
					 ScheduleTransformer.GetPersonAssignmentForLayer(scheduleProjection.SchedulePart, layer);
				row["activity_code"] = activity.Id;
				if (personAssignment.ShiftCategory?.Id != null)
					row["shift_category_code"] = personAssignment.ShiftCategory.Id;
				else
					row["shift_category_code"] = nullGuid;
			}

			row["activity_end"] = layer.Period.EndDateTime;
			row["shift_start"] = shiftStart;
			row["shift_end"] = shiftEnd;
			row["shift_startinterval_id"] = new IntervalBase(shiftStart, intervalsPerDay).Id;
			row["shift_endinterval_id"] = new IntervalBase(shiftEnd.AddSeconds(-1), intervalsPerDay).Id; //remove one seconds to get EndDateTime on correct mart interval

			row["shift_length_m"] = shiftEnd.Subtract(shiftStart).TotalMinutes;

			var timeInfo = new TimeInfo(payload, layer, layerCollection);

			row["scheduled_time_m"] = timeInfo.TotalTime.Time;
			row["scheduled_time_absence_m"] = timeInfo.TotalTime.AbsenceTime;
			row["scheduled_time_activity_m"] = timeInfo.TotalTime.ActivityTime;

			row["scheduled_contract_time_m"] = timeInfo.ContractTime.Time;
			row["scheduled_contract_time_activity_m"] = timeInfo.ContractTime.ActivityTime;
			row["scheduled_contract_time_absence_m"] = timeInfo.ContractTime.AbsenceTime;

			row["scheduled_work_time_m"] = timeInfo.WorkTime.Time;
			row["scheduled_work_time_activity_m"] = timeInfo.WorkTime.ActivityTime;
			row["scheduled_work_time_absence_m"] = timeInfo.WorkTime.AbsenceTime;

			row["scheduled_over_time_m"] = timeInfo.OverTime.Time;

			row["scheduled_ready_time_m"] = 0;
			if (activity != null)
			{
				if (activity.InReadyTime)
				{
					row["scheduled_ready_time_m"] = layer.Period.ElapsedTime().TotalMinutes;
				}
			}

			row["scheduled_paid_time_m"] = timeInfo.PaidTime.Time;
			row["scheduled_paid_time_activity_m"] = timeInfo.PaidTime.ActivityTime;
			row["scheduled_paid_time_absence_m"] = timeInfo.PaidTime.AbsenceTime;
			row["business_unit_code"] = scheduleProjection.SchedulePart.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id;
			row["business_unit_name"] = scheduleProjection.SchedulePart.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Name;
			row["datasource_id"] = 1;
			row["insert_date"] = insertDateTime;
			row["update_date"] = insertDateTime;

			var personAssignmentUpdateOn = DateTime.MinValue;
			if (scheduleProjection.SchedulePart.PersonAssignment() != null)
			{
				personAssignmentUpdateOn = scheduleProjection.SchedulePart.PersonAssignment().UpdatedOn.Value;
			}
			var absenceUpdateOn = DateTime.MinValue;
			foreach (var personAbsence in scheduleProjection.SchedulePart.PersonAbsenceCollection())
			{
				if (personAbsence.UpdatedOn.Value > absenceUpdateOn)
				{
					absenceUpdateOn = personAbsence.UpdatedOn.Value;
				}
			}
			if (absenceUpdateOn > personAssignmentUpdateOn)
				row["datasource_update_date"] = absenceUpdateOn;
			else
				row["datasource_update_date"] = personAssignmentUpdateOn;

			row["overtime_code"] = layer.DefinitionSet?.Id ?? nullGuid;
			row["planned_overtime_m"] = (int)layerCollection.PlannedOvertime(layer.Period).TotalMinutes;

			return row;
		}
	}
}
