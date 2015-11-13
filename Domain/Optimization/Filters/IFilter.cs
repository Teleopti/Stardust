using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public interface IFilter : IEntity
	{
		bool IsValidFor(IPerson person, DateOnly dateOnly);
		string FilterType { get; }
	}
}