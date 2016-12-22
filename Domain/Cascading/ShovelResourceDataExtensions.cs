using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public static class ShovelResourceDataExtensions
	{
		public static IShovelResourceDataForInterval GetDataForInterval(this IShovelResourceData shovelResourceData, ISkill skill, DateTimePeriod period)
		{
			IShovelResourceDataForInterval dataForInterval;
			return shovelResourceData.TryGetDataForInterval(skill, period, out dataForInterval) ?
				dataForInterval :
				new EmptyShovelResourceDataForInterval();
		}
	}
}