﻿using System.Linq;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleTimeHandler
	{
		IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId);
	}

	public class AnalyticsFactScheduleTimeHandler : IAnalyticsFactScheduleTimeHandler
	{
		private readonly IAnalyticsScheduleRepository _repository;

		public AnalyticsFactScheduleTimeHandler(IAnalyticsScheduleRepository repository)
		{
			_repository = repository;
		}

		public IAnalyticsFactScheduleTime Handle(ProjectionChangedEventLayer layer, int shiftCategoryId, int scenarioId)
		{
			var ret = new AnalyticsFactScheduleTime
			{
				ContractTimeMinutes = (int) layer.ContractTime.TotalMinutes,
				WorkTimeMinutes = (int) layer.WorkTime.TotalMinutes,
				ShiftCategoryId = shiftCategoryId,
				ScenarioId = scenarioId
				
			};
			var layerMinutes = (int)(layer.EndDateTime - layer.StartDateTime).TotalMinutes;
			if (!layer.IsAbsence)
			{
				var activities = _repository.Activities();
				var act = activities.FirstOrDefault(a => a.ActivityCode.Equals(layer.PayloadId));
				if (act == null) return ret;
				ret.ActivityId = act.ActivityId;
				ret.ContractTimeActivityMinutes = (int)layer.ContractTime.TotalMinutes;
				ret.WorkTimeActivityMinutes = (int)layer.WorkTime.TotalMinutes;
				if (act.InPaidTime)
				{
					ret.PaidTimeMinutes = layerMinutes;
					ret.PaidTimeActivityMinutes = layerMinutes;
				}
				if (act.InReadyTime)
					ret.ReadyTimeMinues = layerMinutes;
			}
			else
			{
				var absences = _repository.Absences();
				var abs = absences.FirstOrDefault(a => a.AbsenceCode.Equals(layer.PayloadId));
				if (abs == null) return ret;
				ret.AbsenceId = abs.AbsenceId;
				ret.ContractTimeAbsenceMinutes = (int)layer.ContractTime.TotalMinutes;
				ret.WorkTimeAbsenceMinutes = (int)layer.WorkTime.TotalMinutes;
				if (abs.InPaidTime)
				{
					ret.PaidTimeMinutes = layerMinutes;
					ret.PaidTimeAbsenceMinutes = layerMinutes;
				}
			}
			return ret;
		}
	}
}