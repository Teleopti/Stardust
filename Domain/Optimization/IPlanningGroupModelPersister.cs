using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupModelPersister
	{
		void Persist(PlanningGroupModel planningGroupModel);
		void Delete(Guid planningGroupId);
	}
}