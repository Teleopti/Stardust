using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Holds a collection af activities that the master can be converted to.
    ///</summary>
    public interface IMasterActivity : IActivity
    {
        ///<summary>
        /// Replaces the ActivityCollection
        ///</summary>
        ///<param name="newActivityCollection"></param>
        void UpdateActivityCollection(IList<IActivity> newActivityCollection);
    }
}