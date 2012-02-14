﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
    public static class ScheduleDataRowFactory
    {
        public static DataRow CreateScheduleDataRow(DataTable dataTable
                                                  , IVisualLayer layer
                                                  , ScheduleProjection scheduleProjection
                                                  , IntervalBase interval
                                                  , DateTime insertDateTime,
                                                    int intervalsPerDay)
        {
            //DateTimePeriod personPayloadPeriod = scheduleProjection.SchedulePartProjection.ProjectedLayerCollection.Period().Value;
            DateTimePeriod personPayloadPeriod =
                GetShiftPeriod(scheduleProjection.SchedulePartProjectionMerged, layer);
            IPayload payload = layer.Payload;
            DataRow row = dataTable.NewRow();

            row["schedule_date"] = layer.Period.StartDateTime.Date;
            row["person_code"] = scheduleProjection.SchedulePart.Person.Id; // person_code
            row["interval_id"] = interval.Id; // interval_id
            row["activity_start"] = layer.Period.StartDateTime; // activity_start
            row["scenario_code"] = scheduleProjection.SchedulePart.Scenario.Id; // scenario_code

            row["activity_code"] = DBNull.Value;
            row["absence_code"] = DBNull.Value;

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
                row["shift_category_code"] = DBNull.Value;
            }

            if (activity != null)
            {
                IPersonAssignment personAssignment =
                    ScheduleTransformer.GetPersonAssignmentForLayer(scheduleProjection.SchedulePart, layer);
                row["activity_code"] = activity.Id;
                if (personAssignment.MainShift != null && personAssignment.MainShift.ShiftCategory.Id != null)
                    row["shift_category_code"] = personAssignment.MainShift.ShiftCategory.Id;
                else
                    row["shift_category_code"] = DBNull.Value;
            }

            row["activity_end"] = layer.Period.EndDateTime;
            row["shift_start"] = personPayloadPeriod.StartDateTime;
            row["shift_end"] = personPayloadPeriod.EndDateTime;
            row["shift_startinterval_id"] = new IntervalBase(personPayloadPeriod.StartDateTime, intervalsPerDay).Id;

            row["shift_length_m"] = personPayloadPeriod.EndDateTime.Subtract(personPayloadPeriod.StartDateTime).TotalMinutes;

            var timeInfo = new TimeInfo(payload, layer, scheduleProjection.SchedulePartProjection);

            row["scheduled_time_m"] = timeInfo.TotalTime.Time;
            row["scheduled_time_absence_m"] = timeInfo.TotalTime.AbsenceTime;
            row["scheduled_time_activity_m"] = timeInfo.TotalTime.ActivityTime;

            row["scheduled_contract_time_m"] = timeInfo.ContractTime.Time;
            row["scheduled_contract_time_activity_m"] = timeInfo.ContractTime.ActivityTime;
            row["scheduled_contract_time_absence_m"] = timeInfo.ContractTime.AbsenceTime;

            row["scheduled_work_time_m"] = timeInfo.WorkTime.Time;
            row["scheduled_work_time_activity_m"] = timeInfo.WorkTime.ActivityTime;
            row["scheduled_work_time_absence_m"] = timeInfo.WorkTime.AbsenceTime;

            //TimeSpan v =scheduleProjection.SchedulePartProjection.ProjectedLayerCollection.FilterLayers(layer.Period).Overtime();
            //int overtime = 0;
            //if (layer.DefinitionSet != null)
            //    overtime = layer.Period.ElapsedTime().Minutes;
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

            row["last_publish"] = new DateTime(2059, 12, 31);
            row["business_unit_code"] = scheduleProjection.SchedulePart.Scenario.BusinessUnit.Id;
            row["business_unit_name"] = scheduleProjection.SchedulePart.Scenario.BusinessUnit.Name;
            row["datasource_id"] = 1;
            row["insert_date"] = insertDateTime;
            row["update_date"] = insertDateTime;
            row["datasource_update_date"] = insertDateTime;

        	if (timeInfo.OverTime.Time > 0)
        	{
				row["overtime_code"] = layer.DefinitionSet.Id;
        	}
			
            return row;
        }

        public static DateTimePeriod GetShiftPeriod(IVisualLayerCollection layerCollection, ILayer<IPayload> layer )
        {
			//would be nice not doing this for every interval, if it becomes a perf problem - I need to talk with Jonas
			var periods = new List<DateTimePeriod>();
			layerCollection.ForEach(mergedLayer => periods.Add(mergedLayer.Period));

			if (periods.Count == 0)
				throw new ArgumentException("No periods available in layerCollection.");

			if (periods.Count > 1)
			{
				foreach (DateTimePeriod period in periods)
					if (period.Intersect(layer.Period)) return period;
			}

			return (DateTimePeriod)layerCollection.Period();
        }

        public static IPersonAbsence GetPersonAbsenceForLayer(IScheduleDay schedule, ILayer layer)
        {
            IPersonAbsence pAbsence = null;
            IList<IPersonAbsence> pAbsenceCollection = schedule.PersonAbsenceCollection();
            foreach (IPersonAbsence personAbsence in pAbsenceCollection)
            {
                if (personAbsence.Period.Contains(layer.Period))
                {
                    pAbsence = personAbsence;
                    break;
                }
            }
            return pAbsence;
        }


    }
}
