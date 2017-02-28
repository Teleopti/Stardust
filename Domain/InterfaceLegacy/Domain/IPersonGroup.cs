using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Common Interface for RootPersonGroup and ChildPersonGroup
	/// </summary>
	public interface IPersonGroup
	{
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		/// <remarks>
		/// Created by: kosalanp
		/// Created date: 2008-06-23
		/// </remarks>
		Description Description { get; set; }
		/// <summary>
		/// Gets the person collection.
		/// </summary>
		/// <value>The person collection.</value>
		/// <remarks>
		/// Created by: kosalanp
		/// Created date: 2008-06-24
		/// </remarks>
		ReadOnlyCollection<IPerson> PersonCollection { get; }

		/// <summary>
		/// Gets the child collection.
		/// </summary>
		/// <value>The child collection.</value>
		/// <remarks>
		/// Created by: kosalanp
		/// Created date: 2008-06-26
		/// </remarks>
		ReadOnlyCollection<IChildPersonGroup> ChildGroupCollection { get; }
	}
}