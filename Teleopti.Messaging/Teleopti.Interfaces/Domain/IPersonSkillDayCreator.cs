namespace Teleopti.Interfaces.Domain
{
	public interface IPersonSkillDayCreator
	{
		PersonSkillDay Create(DateOnly date, IVirtualSchedulePeriod currentSchedulePeriod);
	}
}