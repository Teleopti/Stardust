using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public interface IUpdateResourceCalculationReadmodel
	{
		void Execute(IScheduleRange scheduleRange);
	}
}