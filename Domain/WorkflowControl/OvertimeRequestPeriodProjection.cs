using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestPeriodProjection : IOvertimeRequestPeriodProjection
	{
		public IList<IOvertimeRequestOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(IWorkflowControlSet workflowControlSet, DateOnlyPeriod requestPeriod)
		{
			var results = new List<IOvertimeRequestOpenPeriod>();
			var filteredPeriods = workflowControlSet.OvertimeRequestOpenPeriods.Select(p => new DateOnlyOvertimeRequestOpenPeriod
			{
				AutoGrantType = p.AutoGrantType,
				Period = p.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse())
			}
			).Where(p => p.Period.Intersection(requestPeriod).HasValue).ToList();

			if (filteredPeriods.IsEmpty())
				return results;

			var startTime = filteredPeriods.Min(p => p.Period.StartDate);
			var endTime = filteredPeriods.Max(d => d.Period.EndDate);
			var currentTime = startTime;

			while (currentTime <= endTime)
			{
				for (var inverseLoopIndex = filteredPeriods.Count - 1; inverseLoopIndex >= 0; inverseLoopIndex--)
				{
					var currentPeriod = filteredPeriods[inverseLoopIndex];
					if (currentPeriod.Period.Contains(currentTime))
					{
						var layerEndTime = findLayerEndTime(filteredPeriods, inverseLoopIndex, currentPeriod, currentTime);
						results.Add(new OvertimeRequestOpenDatePeriod
						{
							AutoGrantType = currentPeriod.AutoGrantType,
							Period = new DateOnlyPeriod(currentTime, layerEndTime)
						});

						if (inverseLoopIndex < filteredPeriods.Count - 1 &&
							layerEndTime == filteredPeriods[inverseLoopIndex + 1].Period.StartDate)
							currentTime = layerEndTime;
						else
							currentTime = layerEndTime.AddDays(1);
						break;
					}
				}
			}

			return results.Where(w => w.GetPeriod(ServiceLocatorForEntity.Now.ServerDate_DontUse()).Intersection(requestPeriod).HasValue).ToList();
		}

		private DateOnly findLayerEndTime(IList<DateOnlyOvertimeRequestOpenPeriod> filteredPeriodsList,
			int currentLayerIndex, DateOnlyOvertimeRequestOpenPeriod workingLayer, DateOnly currentTime)
		{
			DateOnly layerEndTime = workingLayer.Period.EndDate;
			if (currentLayerIndex != filteredPeriodsList.Count - 1)
			{
				var orgLayerCount = filteredPeriodsList.Count;
				for (var higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
				{
					DateOnlyPeriod higherPrioLayerPeriod = filteredPeriodsList[higherPrioLoop].Period;
					if (workingLayer.Period.Contains(higherPrioLayerPeriod.StartDate.AddDays(-1)) &&
						higherPrioLayerPeriod.EndDate > currentTime &&
						higherPrioLayerPeriod.StartDate < layerEndTime)
					{
						layerEndTime = higherPrioLayerPeriod.StartDate.AddDays(-1);
					}
				}
			}
			return layerEndTime;
		}

		private class DateOnlyOvertimeRequestOpenPeriod
		{
			public OvertimeRequestAutoGrantType AutoGrantType { get; set; }
			public DateOnlyPeriod Period { get; set; }
		}
	}

	public interface IOvertimeRequestPeriodProjection
	{
		IList<IOvertimeRequestOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(
			IWorkflowControlSet workflowControlSet, DateOnlyPeriod requestPeriod);
	}
}
