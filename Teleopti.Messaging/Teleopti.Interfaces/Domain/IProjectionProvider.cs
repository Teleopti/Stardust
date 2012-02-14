using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
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
	}

}