using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class NullPlanningGroupSettingsProvider : IPlanningGroupSettingsProvider
	{
		public AllSettingsForPlanningGroup Execute(Guid planningPeriodId)
		{
			return null;
		}
	}
}