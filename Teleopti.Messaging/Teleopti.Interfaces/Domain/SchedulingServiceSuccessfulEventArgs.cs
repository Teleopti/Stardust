using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// EventArgs for SchedulingServiceBase
    /// </summary>
	public class SchedulingServiceSuccessfulEventArgs : SchedulingServiceBaseEventArgs
    {
		public SchedulingServiceSuccessfulEventArgs(IScheduleDay schedulePart, Action cancelAction = null) : base(schedulePart, true, cancelAction)
	    {
	    }
    }
}
