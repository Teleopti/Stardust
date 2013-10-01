using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class Grouping activity
    /// </summary>
    public interface IGroupingActivity : IAggregateRoot
    {
        /// <summary>
        /// Set/Get for description
        /// </summary>     
        Description Description { get; set; }

        /// <summary>
        /// Gets the name part of the description.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-26
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the activities.
        /// Read only wrapper around the actual activity list.
        /// </summary>
        /// <value>The activities list.</value>
        ReadOnlyCollection<IActivity> ActivityCollection { get; }

        /// <summary>
        /// Adds an Activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        void AddActivity(IActivity activity);

        /// <summary>
        /// Remove an Activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        void RemoveActivity(IActivity activity);
    }
}
