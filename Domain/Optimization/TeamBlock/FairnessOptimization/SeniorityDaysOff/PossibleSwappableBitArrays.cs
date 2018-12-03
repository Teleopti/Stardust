using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public class PossibleSwappableDays
	{
		public DateOnly DateForSeniorDayOff { get; set; }
		public DateOnly DateForRemovingSeniorDayOff { get; set; }
	}
}