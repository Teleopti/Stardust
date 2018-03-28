using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IShiftCategorySelector
	{
		IShiftCategory Get(IPerson person, DateOnly date, DateTimePeriod shiftPeriod);
	}
}