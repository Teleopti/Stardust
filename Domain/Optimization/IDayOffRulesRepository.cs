using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesRepository : IRepository<DayOffRules>
	{
		DayOffRules Default();
	}
}