using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public static class SkillStaffPeriodExtensions
	{
		public static double ResourceLoggonOnDiff(this ISkillStaffPeriod skillStaffPeriod)
		{
			return skillStaffPeriod.CalculatedResource - skillStaffPeriod.CalculatedLoggedOn;
		}
	}
}