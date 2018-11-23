using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupSettingsProvider
	{
		AllSettingsForPlanningGroup Execute(Guid planningPeriodId);
	}
}