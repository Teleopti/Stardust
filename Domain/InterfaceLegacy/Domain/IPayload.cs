using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public static class IPayloadExtensions
	{
		public static Description ConfidentialDescription_DONTUSE(this IPayload payload, IPerson assignedPerson)
		{
			return payload.ConfidentialDescription(assignedPerson, ServiceLocator_DONTUSE.CurrentAuthorization, ServiceLocator_DONTUSE.LoggedOnUserIsPerson);
		}
		
		public static Color ConfidentialDisplayColor_DONTUSE(this IPayload payload, IPerson assignedPerson)
		{
			return payload.ConfidentialDisplayColor(assignedPerson, ServiceLocator_DONTUSE.CurrentAuthorization, ServiceLocator_DONTUSE.LoggedOnUserIsPerson);
		}
	}
	
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
		Description ConfidentialDescription(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson);

        /// <summary>
        /// Returns the color of the payload or,
        /// if no permission and payload is secured with confidential flag - return special color
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-02-09
        /// </remarks>
		Color ConfidentialDisplayColor(IPerson assignedPerson, ICurrentAuthorization authorization, ILoggedOnUserIsPerson loggedOnUserIsPerson);

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