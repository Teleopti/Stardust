namespace Teleopti.Interfaces.Domain
{
	public interface IShovelResourceData
	{
		bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval);
	}
}