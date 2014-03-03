using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface IPossibleSwappableDays
	{
		DateOnly DateForSeniorDayOff { get; set; }
		DateOnly DateForRemovingSeniorDayOff { get; set; }
	}
	public class PossibleSwappableDays : IPossibleSwappableDays
	{
		public DateOnly DateForSeniorDayOff { get; set; }
		public DateOnly DateForRemovingSeniorDayOff { get; set; }
	}
}