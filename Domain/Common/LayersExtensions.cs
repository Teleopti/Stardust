using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class LayersExtensions
	{
		public static DateTimePeriod? Period(this IEnumerable<ILayer<IPayload>> layers)
		{
			DateTimePeriod? ret = null;
			foreach (var layer in layers)
			{
				if (layer != null)
				{
					ret = DateTimePeriod.MaximumPeriod(ret, layer.Period);
				}
			}
			return ret;
		}

		public static IVisualLayerCollection CreateProjection(this IMainShiftActivityLayerNew[] layers)
		{
			var projSvc = new VisualLayerProjectionService(null);
			projSvc.Add(layers, new VisualLayerFactory());
			return projSvc.CreateProjection();
		}

		public static IVisualLayerCollection CreateProjection(this IMainShiftActivityLayerNew layer)
		{
			return CreateProjection(new[] {layer});
		}
	}
}