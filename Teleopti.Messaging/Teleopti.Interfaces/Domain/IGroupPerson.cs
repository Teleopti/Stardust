using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGroupPerson: IPerson
    {
        /// <summary>
        /// Return the members in the GroupPerson
        /// </summary>
        /// <returns></returns>
		IEnumerable<IPerson> GroupMembers { get; }

		/// <summary>
		/// Gets or sets the common PossibleStartEndCategory.
		/// </summary>
		/// <value>The common nPossibleStartEndCategory.</value>
		IPossibleStartEndCategory CommonPossibleStartEndCategory  { get; set; }
    }
}
