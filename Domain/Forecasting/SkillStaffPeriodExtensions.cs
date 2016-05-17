using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class SkillStaffPeriodExtensions
	{
		public static void TakeResourcesFrom(this ISkillStaffPeriod thisSkillStaffPeriod, ISkillStaffPeriod otherSkillStaffPeriod, double value)
		{
			thisSkillStaffPeriod.SetCalculatedResource65(thisSkillStaffPeriod.CalculatedResource + value);
			otherSkillStaffPeriod.SetCalculatedResource65(otherSkillStaffPeriod.CalculatedResource - value);
		}
	}
}