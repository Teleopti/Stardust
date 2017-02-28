using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
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