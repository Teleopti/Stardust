using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IFilter : IEntity
	{
		bool IsValidFor(IPerson person, DateOnly dateOnly);
		string FilterType { get; }
	}
}