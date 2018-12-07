using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IScheduleProjection
	{
		IScheduleDay SchedulePart { get; }
		IVisualLayerCollection SchedulePartProjection { get; }
	}
}