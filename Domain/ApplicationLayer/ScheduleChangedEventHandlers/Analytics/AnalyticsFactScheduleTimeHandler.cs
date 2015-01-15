using System;
using System.Linq;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleTimeHandler : IAnalyticsFactScheduleTimeHandler
	{
		private readonly IAnalyticsScheduleRepository _repository;

		public AnalyticsFactScheduleTimeHandler(IAnalyticsScheduleRepository repository)
		{
			_repository = repository;
		}

		public IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId, int shiftLength)
		{
			var ret = new AnalyticsFactScheduleTime
			{
				ContractTimeMinutes = (int) layer.ContractTime.TotalMinutes,
				WorkTimeMinutes = (int) layer.WorkTime.TotalMinutes,
				OverTimeMinutes = (int) layer.Overtime.TotalMinutes,
				OverTimeId = MapOvertimeId(layer.MultiplicatorDefinitionSetId),
				ScenarioId = scenarioId,
				ShiftLengthId = MapShiftLengthId(shiftLength)
				
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
					ret.ReadyTimeMinues = layerMinutes;
			}
			else
			{
				var absence = MapAbsenceId(layer.PayloadId);
				if (absence == null) return ret;
				ret.AbsenceId = absence.AbsenceId;
				ret.ContractTimeAbsenceMinutes = (int)layer.ContractTime.TotalMinutes;
				ret.WorkTimeAbsenceMinutes = (int)layer.WorkTime.TotalMinutes;
				ret.ScheduledAbsenceMinutes = layerMinutes;
				if (absence.InPaidTime)
				{
					ret.PaidTimeMinutes = layerMinutes;
					ret.PaidTimeAbsenceMinutes = layerMinutes;
				}
			}
			return ret;
		}

		public int MapShiftLengthId(int shiftLength)
		{
			var shiftLengths = _repository.ShiftLengths();
			var sl = shiftLengths.FirstOrDefault(a => a.ShiftLength.Equals(shiftLength));
			return sl == null ? _repository.ShiftLengthId(shiftLength) : sl.Id;
		}

		private IAnalyticsActivity mapActivityId(Guid activityCode)
		{
			var activities = _repository.Activities();
			var act = activities.FirstOrDefault(a => a.ActivityCode.Equals(activityCode));
			return act ?? null;
		}

		public IAnalyticsAbsence MapAbsenceId(Guid absenceCode)
		{
			var absences = _repository.Absences();
			var abs = absences.FirstOrDefault(a => a.AbsenceCode.Equals(absenceCode));
			return abs ?? null;
		}

		public int MapOvertimeId(Guid overtimeCode)
		{
			var overtimes = _repository.Overtimes();
			var overtime = overtimes.FirstOrDefault(a => a.Code.Equals(overtimeCode));
			return overtime != null ? overtime.Id : -1;
		}
	}
}