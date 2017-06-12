namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillKeyResource
	{
		public DoubleGuidCombinationKey SkillKey { get; set; }
		public PeriodResourceDetail Resource { get; set; }

		public SkillEffiencyResource[] Effiencies { get; set; }
	}
}