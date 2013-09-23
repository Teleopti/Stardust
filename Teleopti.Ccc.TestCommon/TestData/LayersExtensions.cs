using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class LayersExtensions
	{
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