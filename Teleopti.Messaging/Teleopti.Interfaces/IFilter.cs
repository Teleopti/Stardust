using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IFilter : IEntity
	{
		bool IsValidFor(IPerson person, DateOnly dateOnly);
		string FilterType { get; }
	}
}