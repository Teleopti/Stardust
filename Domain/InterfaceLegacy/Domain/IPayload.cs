using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for payload to be shown
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-22
    /// </remarks>
    public interface IPayload : IAggregateRoot,
                                    IBelongsToBusinessUnit,
                                    IChangeInfo
    {
        /// <summary>
        /// Returns the name of the payload or,
        /// if no permission and payload is secured with confidential flag - return special name
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-02-09
        /// </remarks>
		Description ConfidentialDescription(IPerson assignedPerson);

        /// <summary>
        /// Returns the color of the payload or,
        /// if no permission and payload is secured with confidential flag - return special color
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-02-09
        /// </remarks>
		Color ConfidentialDisplayColor(IPerson assignedPerson);

        /// <summary>
        /// Gets or sets a value indicating whether this payload is [in contract time].
        /// </summary>
        /// <value><c>true</c> if [in contract time]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-27
        /// </remarks>
        bool InContractTime
        { 
            get; 
            set;
        }

        /// <summary>
        /// Gets or sets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        ITracker Tracker
        {
            get; set;
        }

        /// <summary>
        /// Gets the underlying payload (for example the activity for a meeting)
        /// </summary>
        IPayload UnderlyingPayload { get; }
    }
}