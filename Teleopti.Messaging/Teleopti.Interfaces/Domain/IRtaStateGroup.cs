using System;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A grouping of <see cref="IRtaState"/>.
    /// </summary>
    /// <remarks>
    /// Created by: Jonas N
    /// Created date: 2008-10-03
    /// </remarks>
    public interface IRtaStateGroup : IPayload
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the state group is the default one.
        /// </summary>
        /// <value><c>true</c> if state group is the default one; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        bool DefaultStateGroup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an agent in a state, that belongs to this state group, is considered as available.
        /// </summary>
        /// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        bool Available { get; set; }

        /// <summary>
        /// Gets the state collection of states that belongs to this state group.
        /// </summary>
        /// <value>The state collection.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        ReadOnlyCollection<IRtaState> StateCollection { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is log out state.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is log out state; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-21
        /// </remarks>
        bool IsLogOutState { get; set; }

        /// <summary>
        /// Creates new state and that belongs to this state group.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <param name="stateCode">The state code from a specific ACD platform.</param>
        /// <param name="platformTypeId">A static Guid that shows which platform the state comes from.</param>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-10-03
        /// </remarks>
        IRtaState AddState(string stateName, string stateCode, Guid platformTypeId);

        /// <summary>
        /// Moves the state to supplied target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="state">The state.</param>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2008-10-09
        /// </remarks>
        IRtaState MoveStateTo(IRtaStateGroup target, IRtaState state);

		/// <summary>
		///  Deletes the State completely from the Db.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <remarks>
		/// Created by: Ola
		/// Created date: 2012-12-21
		/// </remarks>
    	void DeleteState(IRtaState state);
    }
}