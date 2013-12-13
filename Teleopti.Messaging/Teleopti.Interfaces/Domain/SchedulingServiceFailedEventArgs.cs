namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// EventArgs for SchedulingServiceFailedEvent
    /// </summary>
	public class SchedulingServiceFailedEventArgs : SchedulingServiceBaseEventArgs
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="SchedulingServiceFailedEventArgs"/> class.
	    /// </summary>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2009-01-14
	    /// </remarks>
		public SchedulingServiceFailedEventArgs() : 
			base(null, false)
	    {
	    }
    }
}
