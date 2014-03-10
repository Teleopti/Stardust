using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FactSchedule : IAnalyticsDataSetup
	{
		private readonly int _shiftStartdateLocalId;
		private readonly int _scheduleDateId;
		private readonly int _personId;
		private readonly int _intervalId;
		private readonly DateTime _activityStarttime;
		private readonly int _scenarioId;
		private readonly int _activityId;
		private readonly int _absenceId;
		private readonly int _activityStartdateId;
		private readonly int _activityEnddateId;
		private readonly DateTime _activityEndtime;
		private readonly int _shiftStartdateId;
		private readonly DateTime _shiftStarttime;
		private readonly int _shiftEnddateId;
		private readonly DateTime _shiftEndtime;
		private readonly int _shiftStartintervalId;
		private readonly int _shiftEndintervalId;
		private readonly int _shiftCategoryId;
		private readonly int _shiftLengthId;
		private readonly int _scheduledTimeM;
		private readonly int _scheduledTimeAbsenceM;
		private readonly int _scheduledTimeActivityM;
		private readonly int _scheduledContractTimeM;
		private readonly int _scheduledContractTimeActivityM;
		private readonly int _scheduledContractTimeAbsenceM;
		private readonly int _scheduledWorkTimeM;
		private readonly int _scheduledWorkTimeActivityM;
		private readonly int _scheduledWorkTimeAbsenceM;
		private readonly int _scheduledOverTimeM;
		private readonly int _scheduledReadyTimeM;
		private readonly int _scheduledPaidTimeM;
		private readonly int _scheduledPaidTimeActivityM;
		private readonly int _scheduledPaidTimeAbsenceM;
		private readonly int _businessUnitId;

		public FactSchedule(
			int shift_startdate_local_id,
			int schedule_date_id,
			int person_id,
			int interval_id,
			DateTime activity_starttime,
			int scenario_id,
			int activity_id,
			int absence_id,
			int activity_startdate_id,
			int activity_enddate_id,
			DateTime activity_endtime,
			int shift_startdate_id,
			DateTime shift_starttime,
			int shift_enddate_id,
			DateTime shift_endtime,
			int shift_startinterval_id,
			int shift_endinterval_id,
			int shift_category_id,
			int shift_length_id,
			int scheduled_time_m,
			int scheduled_time_absence_m,
			int scheduled_time_activity_m,
			int scheduled_contract_time_m,
			int scheduled_contract_time_activity_m,
			int scheduled_contract_time_absence_m,
			int scheduled_work_time_m,
			int scheduled_work_time_activity_m,
			int scheduled_work_time_absence_m,
			int scheduled_over_time_m,
			int scheduled_ready_time_m,
			int scheduled_paid_time_m,
			int scheduled_paid_time_activity_m,
			int scheduled_paid_time_absence_m,
			int business_unit_id)
		{
			_shiftStartdateLocalId = shift_startdate_local_id;
			_scheduleDateId = schedule_date_id;
			_personId = person_id;
			_intervalId = interval_id;
			_activityStarttime = activity_starttime;
			_scenarioId = scenario_id;
			_activityId = activity_id;
			_absenceId = absence_id;
			_activityStartdateId = activity_startdate_id;
			_activityEnddateId = activity_enddate_id;
			_activityEndtime = activity_endtime;
			_shiftStartdateId = shift_startdate_id;
			_shiftStarttime = shift_starttime;
			_shiftEnddateId = shift_enddate_id;
			_shiftEndtime = shift_endtime;
			_shiftStartintervalId = shift_startinterval_id;
			_shiftEndintervalId = shift_endinterval_id;
			_shiftCategoryId = shift_category_id;
			_shiftLengthId = shift_length_id;
			_scheduledTimeM = scheduled_time_m;
			_scheduledTimeAbsenceM = scheduled_time_absence_m;
			_scheduledTimeActivityM = scheduled_time_activity_m;
			_scheduledContractTimeM = scheduled_contract_time_m;
			_scheduledContractTimeActivityM = scheduled_contract_time_activity_m;
			_scheduledContractTimeAbsenceM = scheduled_contract_time_absence_m;
			_scheduledWorkTimeM = scheduled_work_time_m;
			_scheduledWorkTimeActivityM = scheduled_work_time_activity_m;
			_scheduledWorkTimeAbsenceM = scheduled_work_time_absence_m;
			_scheduledOverTimeM = scheduled_over_time_m;
			_scheduledReadyTimeM = scheduled_ready_time_m;
			_scheduledPaidTimeM = scheduled_paid_time_m;
			_scheduledPaidTimeActivityM = scheduled_paid_time_activity_m;
			_scheduledPaidTimeAbsenceM = scheduled_paid_time_absence_m;
			_businessUnitId = business_unit_id;
		}

		public FactSchedule(int personId, int shiftStartdateLocalId, int scheduleDateId, int scheduledTimeM, int scheduledReadyTimeMinutes, int intervalId, int scenarioId)
		{
			_personId = personId;
			_shiftStartdateLocalId = shiftStartdateLocalId;
			_scheduleDateId = scheduleDateId;
			_scheduledTimeM = scheduledTimeM;
			_scheduledReadyTimeM = scheduledReadyTimeMinutes;
			_intervalId = intervalId;
			_scenarioId = scenarioId;
			_activityStarttime = DateTime.Now;
			_activityEndtime = DateTime.Now;
			_shiftStarttime = DateTime.Now;
			_shiftEndtime = DateTime.Now;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = fact_schedule.CreateTable())
			{
				table.AddFactSchedule(
					_shiftStartdateLocalId,
					_scheduleDateId,
					_personId,
					_intervalId,
					_activityStarttime,
					_scenarioId,
					_activityId,
					_absenceId,
					_activityStartdateId,
					_activityEnddateId,
					_activityEndtime,
					_shiftStartdateId,
					_shiftStarttime,
					_shiftEnddateId,
					_shiftEndtime,
					_shiftStartintervalId,
					_shiftEndintervalId,
					_shiftCategoryId,
					_shiftLengthId,
					_scheduledTimeM,
					_scheduledTimeAbsenceM,
					_scheduledTimeActivityM,
					_scheduledContractTimeM,
					_scheduledContractTimeActivityM,
					_scheduledContractTimeAbsenceM,
					_scheduledWorkTimeM,
					_scheduledWorkTimeActivityM,
					_scheduledWorkTimeAbsenceM,
					_scheduledOverTimeM,
					_scheduledReadyTimeM,
					_scheduledPaidTimeM,
					_scheduledPaidTimeActivityM,
					_scheduledPaidTimeAbsenceM,
					_businessUnitId);
				Bulk.Insert(connection, table);
			}
		}
	}
}