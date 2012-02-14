using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class ScheduleProjection
    {
        private readonly IVisualLayerCollection _projection;
        private readonly IScheduleDay _schedulePart;
		private readonly IVisualLayerCollection _schedulePartProjectionMerged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ScheduleProjection(IScheduleDay schedule, IVisualLayerCollection projection)
        {
            _schedulePart = schedule;
            _projection = projection;

			_schedulePartProjectionMerged = new VisualLayerCollection(projection.Person,
												 projection.ToList<IVisualLayer>(),
												 new ProjectionIntersectingPeriodMerger());
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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