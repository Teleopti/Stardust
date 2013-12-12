using System.ComponentModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// EventArgs for SchedulingServiceBase
    /// </summary>
    public class SchedulingServiceBaseEventArgs : CancelEventArgs
    {
        private readonly IScheduleDay _schedulePart;
		private readonly bool _isSuccessful;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingServiceBaseEventArgs"/> class.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
		/// <param name="isSuccessful">Scheduled succeeded or not.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-14
        /// </remarks>
		public SchedulingServiceBaseEventArgs(IScheduleDay schedulePart, bool isSuccessful)
        {
	        _schedulePart = schedulePart;
	        _isSuccessful = isSuccessful;
        }

	    /// <summary>
        /// Gets the schedule part.
        /// </summary>
        /// <value>The schedule part.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-14
        /// </remarks>
        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }

		public bool IsSuccessful
		{
			get { return _isSuccessful; }
		}
    }
}
