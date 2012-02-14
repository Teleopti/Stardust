using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Keeps track of the changed SchedulePart
    /// </summary>
    public class SchedulePartTracker : ISchedulePartTracker
    {
        private IScheduleDay _originalPart;
        private IScheduleDay _changedPart;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDayTracker"/> class.
        /// </summary>
        /// <param name="originalPart">The original part.</param>
        /// <param name="changedPart">The changed part.</param>
        public SchedulePartTracker(IScheduleDay originalPart, IScheduleDay changedPart)
        {
            OriginalPart = originalPart;
            ChangedPart = changedPart;
        }

        /// <summary>
        /// Gets or sets the original part.
        /// </summary>
        /// <value>The original part.</value>
        public IScheduleDay OriginalPart
        {
            get { return _originalPart; }
            set { _originalPart = value; }
        }

        /// <summary>
        /// Gets or sets the changed part.
        /// </summary>
        /// <value>The changed part.</value>
        public IScheduleDay ChangedPart
        {
            get { return _changedPart; }
            set { _changedPart = value; }
        }
    }
}
