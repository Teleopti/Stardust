using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Swap service new
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface ISwapServiceNew
	{
		IList<IScheduleDay> Swap(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2, IScheduleDictionary schedules);
	}
}
