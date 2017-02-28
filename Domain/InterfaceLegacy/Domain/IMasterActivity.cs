using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Holds a collection af activities that the master can be converted to.
    ///</summary>
    public interface IMasterActivity : IActivity
    {
        ///<summary>
        /// Gets all the activities that the master can be converted to when scheduling.
        ///</summary>
        IList<IActivity> ActivityCollection { get; }

        ///<summary>
        /// Replaces the ActivityCollection
        ///</summary>
        ///<param name="newActivityCollection"></param>
        void UpdateActivityCollection(IList<IActivity> newActivityCollection);
    }
}