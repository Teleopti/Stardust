using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public static class ShovelResourceDataExtensions
	{
		public static IShovelResourceDataForInterval GetDataForInterval(this IShovelResourceData shovelResourceData, ISkill skill, DateTimePeriod period)
		{
			return shovelResourceData.TryGetDataForInterval(skill, period, out var dataForInterval) ?
				dataForInterval :
				new EmptyShovelResourceDataForInterval();
		}
	}
}