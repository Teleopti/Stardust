using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// ScheduleTag
    /// </summary>
    public interface IScheduleTag : IAggregateRoot, IChangeInfo, IDeleteTag
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; set; }
    }
}
