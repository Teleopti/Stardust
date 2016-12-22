namespace Teleopti.Interfaces.Domain
{
	public interface IShovelResourceData
	{
		bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval);

		//note - should never throw - either return real one or a null object with all data set to zero
		IShovelResourceDataForInterval GetDataForInterval(ISkill skill, DateTimePeriod period);
	}
}