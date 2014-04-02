

using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IVisualLayerToBaseDateMapper
	{
		TimeSpan Map(IVisualLayer layerToMap, IVisualLayer firstLayerInShift, int defaultResolution);
	}

	public class VisualLayerToBaseDateMapper : IVisualLayerToBaseDateMapper
	{
		public TimeSpan Map(IVisualLayer layerToMap, IVisualLayer firstLayerInShift, int defaultResolution)
		{
			var fittedTimeSpan = TimeHelper.FitToDefaultResolutionRoundDown(layerToMap.Period.StartDateTime.TimeOfDay, defaultResolution);
			var dateOfTheFirstLayer = firstLayerInShift.Period.StartDateTime.Date;
			var offsetDays = layerToMap.Period.StartDateTime.Date.Subtract(dateOfTheFirstLayer.Date).TotalDays;
			var adjustedTimeSpan = fittedTimeSpan.Add(TimeSpan.FromDays(offsetDays));
			return adjustedTimeSpan;
		}
	}
}