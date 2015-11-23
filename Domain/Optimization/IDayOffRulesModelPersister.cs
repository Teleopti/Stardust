using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesModelPersister
	{
		void Persist(DayOffRulesModel model);
		void Delete(Guid id);
	}
}