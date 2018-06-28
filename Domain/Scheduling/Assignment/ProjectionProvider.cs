using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ProjectionProvider : IProjectionProvider
	{
		public IVisualLayerCollection Projection(IScheduleDay scheduleDay)
		{
			return scheduleDay.ProjectionService().CreateProjection();
		}
		public IEnumerable<IVisualLayer> UnmergedProjection(IScheduleDay scheduleDay)
		{
			return ((VisualLayerCollection)scheduleDay.ProjectionService().CreateProjection()).UnMergedCollection;
		}
	}

}