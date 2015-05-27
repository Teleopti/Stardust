using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IScheduleProjection
	{
		IScheduleDay SchedulePart { get; }
		IVisualLayerCollection SchedulePartProjection { get; }
		IVisualLayerCollection SchedulePartProjectionMerged { get; }
	}
}