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
		/// Gets or sets the common shift category.
		/// </summary>
		/// <value>The common shift category.</value>
    	IShiftCategory CommonShiftCategory { get; set; }
    }
}
