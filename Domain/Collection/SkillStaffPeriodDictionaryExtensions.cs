using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public static class SkillStaffPeriodDictionaryExtensions
	{
		public static ISkillStaffPeriod SkillStaffPeriodOrDefault(this ISkillStaffPeriodDictionary skillStaffPeriodDictionary, DateTimePeriod period)
		{
			ISkillStaffPeriod skillStaffPeriod;
			return skillStaffPeriodDictionary.TryGetValue(period, out skillStaffPeriod) ? 
				skillStaffPeriod :
				createNullObject(period);
		}

		public static ISkillStaffPeriod SkillStaffPeriodOrDefault(this ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, ISkill skill, DateTimePeriod period)
		{
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary;
			return skillSkillStaffPeriodExtendedDictionary.TryGetValue(skill, out skillStaffPeriodDictionary) ?
				SkillStaffPeriodOrDefault(skillStaffPeriodDictionary, period) : 
				createNullObject(period);
		}

		private static ISkillStaffPeriod createNullObject(DateTimePeriod period)
		{
			return new SkillStaffPeriod(period, new Task(), new ServiceAgreement(), new StaffingCalculatorServiceFacade());
		}
	}
}