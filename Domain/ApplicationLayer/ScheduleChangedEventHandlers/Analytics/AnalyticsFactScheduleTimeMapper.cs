using System;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleTimeMapper
	{
		IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId, int shiftLength, TimeSpan plannedOvertime);
		AnalyticsAbsence MapAbsenceId(Guid absenceCode);
		int MapOvertimeId(Guid overtimeCode);
		int MapShiftLengthId(int shiftLength);
	}

	public class AnalyticsFactScheduleTimeMapper : IAnalyticsFactScheduleTimeMapper
	{
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IAnalyticsAbsenceRepository _analyticsAbsenceRepository;
		private readonly IAnalyticsActivityRepository _analyticsActivityRepository;
		private readonly IAnalyticsOvertimeRepository _analyticsOvertimeRepository;

		public AnalyticsFactScheduleTimeMapper(IAnalyticsScheduleRepository analyticsScheduleRepository, 
			IAnalyticsAbsenceRepository analyticsAbsenceRepository, 
			IAnalyticsOvertimeRepository analyticsOvertimeRepository,
			IAnalyticsActivityRepository analyticsActivityRepository)
		{
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_analyticsAbsenceRepository = analyticsAbsenceRepository;
			_analyticsActivityRepository = analyticsActivityRepository;
			_analyticsOvertimeRepository = analyticsOvertimeRepository;
		}

		public IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId, int shiftLength, TimeSpan plannedOvertime)
		{
			
			var ret = new AnalyticsFactScheduleTime
			{
				ContractTimeMinutes = (int) layer.ContractTime.TotalMinutes,
				WorkTimeMinutes = (int) layer.WorkTime.TotalMinutes,
				OverTimeMinutes = (int) layer.Overtime.TotalMinutes,
				OverTimeId = MapOvertimeId(layer.MultiplicatorDefinitionSetId),
				ScenarioId = scenarioId,
				ShiftLengthId = MapShiftLengthId(shiftLength),
				PlannedOvertimeMinutes = (int)plannedOvertime.TotalMinutes
			};
			var layerMinutes = (int)(layer.EndDateTime - layer.StartDateTime).TotalMinutes;
			ret.ScheduledMinutes = layerMinutes;
			if (!layer.IsAbsence)
			{
				ret.ShiftCategoryId = shiftCategoryId;

				var activity = mapActivityId(layer.PayloadId);
				if (activity == null) return ret;
				ret.ActivityId = activity.ActivityId;
				ret.ContractTimeActivityMinutes = (int)layer.ContractTime.TotalMinutes;
				ret.WorkTimeActivityMinutes = (int)layer.WorkTime.TotalMinutes;
				ret.ScheduledActivityMinutes = layerMinutes;
				if (activity.InPaidTime)
				{
					ret.PaidTimeMinutes = layerMinutes;
					ret.PaidTimeActivityMinutes = layerMinutes;
				}
				if (activity.InReadyTime)
					ret.ReadyTimeMinutes = layerMinutes;
			}
			else
			{
				var absence = MapAbsenceId(layer.PayloadId);
				if (absence == null) return ret;
				ret.AbsenceId = absence.AbsenceId;
				ret.ContractTimeAbsenceMinutes = (int)layer.ContractTime.TotalMinutes;
				ret.WorkTimeAbsenceMinutes = (int)layer.WorkTime.TotalMinutes;
				ret.ScheduledAbsenceMinutes = layerMinutes;
				ret.PaidTimeMinutes = (int)layer.PaidTime.TotalMinutes;
				ret.PaidTimeAbsenceMinutes = (int)layer.PaidTime.TotalMinutes;
				
			}
			return ret;
		}

		public int MapShiftLengthId(int shiftLength)
		{
			var shiftLengths = _analyticsScheduleRepository.ShiftLengths();
			var sl = shiftLengths.FirstOrDefault(a => a.ShiftLength.Equals(shiftLength));
			return sl?.Id ?? _analyticsScheduleRepository.ShiftLengthId(shiftLength);
		}

		private AnalyticsActivity mapActivityId(Guid activityCode)
		{
			var activities = _analyticsActivityRepository.Activities();
			var act = activities.FirstOrDefault(a => a.ActivityCode.Equals(activityCode));
			return act;
		}

		public AnalyticsAbsence MapAbsenceId(Guid absenceCode)
		{
			var absences = _analyticsAbsenceRepository.Absences();
			var abs = absences.FirstOrDefault(a => a.AbsenceCode.Equals(absenceCode));
			return abs;
		}

		public int MapOvertimeId(Guid overtimeCode)
		{
			var overtimes = _analyticsOvertimeRepository.Overtimes();
			var overtime = overtimes.FirstOrDefault(a => a.OvertimeCode.Equals(overtimeCode));
			return overtime?.OvertimeId ?? -1;
		}
	}
}