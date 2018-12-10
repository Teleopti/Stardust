namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IShovelResourceData
	{
		bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval);
	}
}