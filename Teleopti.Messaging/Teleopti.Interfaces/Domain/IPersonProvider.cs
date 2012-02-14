using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Provides the persons in the organization
    /// </summary>
    public interface IPersonProvider
    {
        /// <summary>
        /// Provides the persons in the organization.
        /// </summary>
        /// <returns></returns>
        IList<IPerson> GetPersons();

        ///<summary>
        /// Flag to know if the Schedule data should load by persons or simply for all persons.
        ///</summary>
        bool DoLoadByPerson { get; set; }
        
        /////<summary>
        ///// Flag to know if the Restrictions (Rotation, Avalaibilties etc.) should be loaded. Default True.
        /////</summary>
        //bool LoadRestrictions { get; set; }

        /////<summary>
        ///// Flag to know if the Notes, Public and Private, should be loaded. Default True.
        /////</summary>
        //bool LoadNotes { get; set; }
    }
}