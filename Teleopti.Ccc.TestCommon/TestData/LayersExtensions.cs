using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class LayersExtensions
	{
		private static readonly VisualLayerFactory visualLayerFactory = new VisualLayerFactory();

		public static IVisualLayerCollection CreateProjection(this IEnumerable<MainShiftLayer> layers)
		{
			var projSvc = new VisualLayerProjectionService();
			layers.ForEach(l => projSvc.Add(l, visualLayerFactory));
			return projSvc.CreateProjection();
		}

		public static IVisualLayerCollection CreateProjection(this MainShiftLayer layer)
		{
			return CreateProjection(new[] {layer});
		}
	}
}