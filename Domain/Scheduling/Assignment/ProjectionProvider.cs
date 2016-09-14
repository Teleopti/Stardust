using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ProjectionProvider : IProjectionProvider
	{
		public IVisualLayerCollection Projection(IScheduleDay scheduleDay)
		{
			return scheduleDay.ProjectionService().CreateProjection();
		}
		public IVisualLayer[] UnmergedProjection(IScheduleDay scheduleDay)
		{
			return ((VisualLayerCollection)scheduleDay.ProjectionService().CreateProjection()).UnMergedCollection;
		}
	}

}