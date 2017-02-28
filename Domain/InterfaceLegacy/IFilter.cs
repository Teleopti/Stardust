using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IFilter : IEntity
	{
		bool IsValidFor(IPerson person, DateOnly dateOnly);
		string FilterType { get; }
	}
}