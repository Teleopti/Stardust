using System.Collections.ObjectModel;

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
		ReadOnlyCollection<IPerson> GroupMembers { get; }

		/// <summary>
		/// Gets or sets the common PossibleStartEndCategory.
		/// </summary>
		/// <value>The common nPossibleStartEndCategory.</value>
		IPossibleStartEndCategory CommonPossibleStartEndCategory  { get; set; }
    }
}
