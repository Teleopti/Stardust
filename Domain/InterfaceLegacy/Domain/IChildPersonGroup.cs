namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Interface for ChildPersonGroup
    ///</summary>
    public interface IChildPersonGroup : IAggregateEntity, IPersonGroup
    {
        ///<summary>
        /// The entity this group is based on.
        ///</summary>
        IAggregateRoot Entity { get; set; }

        ///<summary>
        /// Determines if this group equals a team in the organization hierarchy.
        ///</summary>
        bool IsTeam { get; }

        ///<summary>
        /// Determines if this group equals a site in the organization hierarchy.
        ///</summary>
        bool IsSite { get; }

		///<summary>
		/// Determines if this group is based on an optional column.
		///</summary>
		bool IsOptionalColumn { get; }

	    /// <summary>
        /// Adds the person to group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-23
        /// </remarks>
        void AddPerson(IPerson person);

        /// <summary>
        /// Removes the person from group.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-23
        /// </remarks>
        void RemovePerson(IPerson person);

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="group">The child.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        void AddChildGroup(IChildPersonGroup group);

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <param name="group">The child.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        void RemoveChildGroup(IChildPersonGroup group);
    }
}