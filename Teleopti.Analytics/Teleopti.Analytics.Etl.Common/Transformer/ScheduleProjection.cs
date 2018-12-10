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

		public ScheduleProjection(IScheduleDay schedule, IVisualLayerCollection projection)
		{
			_schedulePart = schedule;
			_projection = projection;
		}

		public IScheduleDay SchedulePart
		{
			get { return _schedulePart; }
		}

		public IVisualLayerCollection SchedulePartProjection
		{
			get { return _projection; }
		}
	}
}