using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleTimeMapper
	{
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly AnalyticsAbsenceMapper _analyticsAbsenceMapper;
		private readonly IAnalyticsOvertimeRepository _analyticsOvertimeRepository;

		public AnalyticsFactScheduleTimeMapper(IAnalyticsScheduleRepository analyticsScheduleRepository, 
			AnalyticsAbsenceMapper analyticsAbsenceMapper, 
			IAnalyticsOvertimeRepository analyticsOvertimeRepository)
		{
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_analyticsAbsenceMapper = analyticsAbsenceMapper;
			_analyticsOvertimeRepository = analyticsOvertimeRepository;
		}

		public IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer,
			IDictionary<Guid, AnalyticsActivity> activityMapping, int shiftCategoryId, int scenarioId, int shiftLengthId,
			TimeSpan plannedOvertime)
		{
			
			var ret = new AnalyticsFactScheduleTime
			{
				ContractTimeMinutes = (int) layer.ContractTime.TotalMinutes,
				WorkTimeMinutes = (int) layer.WorkTime.TotalMinutes,
				OverTimeMinutes = (int) layer.Overtime.TotalMinutes,
				OverTimeId = layer.MultiplicatorDefinitionSetId == Guid.Empty ? -1 : MapOvertimeId(layer.MultiplicatorDefinitionSetId),
				ScenarioId = scenarioId,
				ShiftLengthId = shiftLengthId,
				PlannedOvertimeMinutes = (int)plannedOvertime.TotalMinutes
			};
			var layerMinutes = (int)(layer.EndDateTime - layer.StartDateTime).TotalMinutes;
			ret.ScheduledMinutes = layerMinutes;
			if (!layer.IsAbsence)
			{
				ret.ShiftCategoryId = shiftCategoryId;

				if (!activityMapping.TryGetValue(layer.PayloadId, out var activity)) return ret;

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
			return _analyticsScheduleRepository.ShiftLengthId(shiftLength);
		}
		
		public AnalyticsAbsence MapAbsenceId(Guid absenceCode)
		{
			return _analyticsAbsenceMapper.Map(absenceCode);
		}

		public int MapOvertimeId(Guid overtimeCode)
		{
			var overtimes = _analyticsOvertimeRepository.Overtimes();
			var overtime = overtimes.FirstOrDefault(a => a.OvertimeCode.Equals(overtimeCode));
			return overtime?.OvertimeId ?? -1;
		}
	}
}