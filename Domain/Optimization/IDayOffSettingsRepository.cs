using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffSettingsRepository : IRepository<DayOffSettings>
	{
		DayOffSettings Default();
	}
}