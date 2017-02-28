using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Details for a custom message about a changed entity.
	/// </summary>
	public interface ICustomChangedEntity : IPeriodized,IMainReference
	{
		/// <summary>
		/// Get the id of the changed instance.
		/// </summary>
		Guid? Id { get; }
	}
}