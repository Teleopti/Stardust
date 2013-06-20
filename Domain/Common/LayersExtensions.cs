using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class LayersExtensions
	{
		public static IEnumerable<DateTimePeriod> PeriodBlocks(this IEnumerable<ILayer<IPayload>> layers)
		{
			var blockPeriods = new List<DateTimePeriod>();
			var sortedOnStartDate = layers.ToList();
			sortedOnStartDate.Sort(new SortStartDate());
			foreach (var layer in sortedOnStartDate)
			{
				var layerPeriod = layer.Period;
				for (var i = blockPeriods.Count - 1; i >= 0; i--)
				{
					var blockPeriod = blockPeriods[i];
					var blockPeriodLayerIntersection = blockPeriod.Intersection(layerPeriod);
					if (blockPeriodLayerIntersection.HasValue)
					{
						blockPeriods.Remove(blockPeriod);
						blockPeriods.Add(blockPeriodLayerIntersection.Value);
					}
				}
				blockPeriods.Add(layerPeriod);
			}
			return blockPeriods;
		}

		public static IVisualLayerCollection CreateProjection(this IEnumerable<IMainShiftLayer> layers)
		{
			var projSvc = new VisualLayerProjectionService(null);
			projSvc.Add(layers, new VisualLayerFactory());
			return projSvc.CreateProjection();
		}

		public static IVisualLayerCollection CreateProjection(this IMainShiftLayer layer)
		{
			return CreateProjection(new[] {layer});
		}
	}
}