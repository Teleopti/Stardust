using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Creates a projection
	/// </summary>
	public interface IProjectionProvider
	{
		/// <summary>
		/// Creates a projection of a schedule day
		/// </summary>
		/// <param name="scheduleDay">The scheduleday</param>
		/// <returns>The projection</returns>
		IVisualLayerCollection Projection(IScheduleDay scheduleDay);

		IEnumerable<IVisualLayer> UnmergedProjection(IScheduleDay scheduleDay);
	}
}