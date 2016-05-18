using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public static class SkillStaffPeriodDictionaryExtensions
	{
		public static ISkillStaffPeriod SkillStaffPeriodOrDefault(this ISkillStaffPeriodDictionary skillStaffPeriodDictionary, DateTimePeriod period)
		{
			ISkillStaffPeriod skillStaffPeriod;
			return !skillStaffPeriodDictionary.TryGetValue(period, out skillStaffPeriod) ? 
				new SkillStaffPeriod(period, new Task(), new ServiceAgreement(), new StaffingCalculatorServiceFacade()) : 
				skillStaffPeriod;
		}
	}
}