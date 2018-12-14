using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupSettingsModelPersister
	{
		void Persist(PlanningGroupSettingsModel model);
		void Delete(Guid id);

		void UpdateDefaultSetting(PlanningGroupSettings planningGroupSettings,
			PlanningGroupSettingsModel planningGroupSettingsModel);
	}
}