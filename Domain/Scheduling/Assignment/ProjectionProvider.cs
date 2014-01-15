using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ProjectionProvider : IProjectionProvider
	{
		public IVisualLayerCollection Projection(IScheduleDay scheduleDay)
		{
			return scheduleDay.ProjectionService().CreateProjection();
		}
	}

}