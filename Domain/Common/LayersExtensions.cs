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
			var allPeriods = layers.Select(l => l.Period);
			return DateTimePeriod.MergePeriods(allPeriods);
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