using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Options to set behavior when creating new group page instances
    ///</summary>
    public interface IGroupPageOptions
    {
        ///<summary>
        /// The persons available
        ///</summary>
        IEnumerable<IPerson> Persons { get; }

        ///<summary>
        /// The name (translated) for this group page
        ///</summary>
        string CurrentGroupPageName { get; set; }

        ///<summary>
        /// The resource key to use for the group page name
        /// Primarily used in ETL processing
        ///</summary>
        string CurrentGroupPageNameKey { get; set; }

        ///<summary>
        /// The period to view the group page for
        ///</summary>
        /// <remarks>
        /// The default value is Today (only)
        /// </remarks>
        DateOnlyPeriod SelectedPeriod { get; set; }
    }
}