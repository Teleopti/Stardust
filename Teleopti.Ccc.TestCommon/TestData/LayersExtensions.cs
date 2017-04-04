using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class LayersExtensions
	{
		public static IVisualLayerCollection CreateProjection(this IEnumerable<MainShiftLayer> layers)
		{
			var projSvc = new VisualLayerProjectionService(null);
			projSvc.Add(layers, new VisualLayerFactory());
			return projSvc.CreateProjection();
		}

		public static IVisualLayerCollection CreateProjection(this MainShiftLayer layer)
		{
			return CreateProjection(new[] {layer});
		}
	}
}