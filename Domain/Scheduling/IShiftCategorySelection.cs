using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IShiftCategorySelection : IAggregateRoot
	{
		string Model { get; set; }
	}
}