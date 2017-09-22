using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleProjection : IScheduleProjection
	{
		private readonly IVisualLayerCollection _projection;
		private readonly IScheduleDay _schedulePart;
		private readonly IVisualLayerCollection _schedulePartProjectionMerged;

		public ScheduleProjection(IScheduleDay schedule, IVisualLayerCollection projection)
		{
			_schedulePart = schedule;
			_projection = projection;

			_schedulePartProjectionMerged = new VisualLayerCollection(projection.Person,
												 projection.ToList(),
												 new ProjectionIntersectingPeriodMerger());
		}


		public IScheduleDay SchedulePart
		{
			get { return _schedulePart; }
		}

		public IVisualLayerCollection SchedulePartProjection
		{
			get { return _projection; }
		}

		public IVisualLayerCollection SchedulePartProjectionMerged
		{
			get { return _schedulePartProjectionMerged; }
		}
	}
}