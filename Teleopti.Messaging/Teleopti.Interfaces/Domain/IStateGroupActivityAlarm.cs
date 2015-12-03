namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Connect an alarm type to a state group / activity combination
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-11-14
    /// </remarks>
	public interface IStateGroupActivityAlarm : IAggregateRoot, ICloneableEntity<IStateGroupActivityAlarm>
    {
        /// <summary>
        /// Gets the activity.
        /// </summary>
        /// <value>The activity.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-14
        /// </remarks>
        IActivity Activity { get; }

        /// <summary>
        /// Gets the state group.
        /// </summary>
        /// <value>The state group.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-14
        /// </remarks>
        IRtaStateGroup StateGroup { get; }

        /// <summary>
        /// Gets or sets the type of the alarm.
        /// </summary>
        /// <value>The type of the alarm.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-14
        /// </remarks>
        IRtaRule RtaRule { get; set; }
    }
}