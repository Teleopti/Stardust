using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Common Interface for RootPersonGroup and ChildPersonGroup
	/// </summary>
	public interface IPersonGroup
	{
		string Name { get; set; }

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