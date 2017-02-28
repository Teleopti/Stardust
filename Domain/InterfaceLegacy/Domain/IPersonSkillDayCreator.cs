using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonSkillDayCreator
	{
		PersonSkillDay Create(DateOnly date, IVirtualSchedulePeriod currentSchedulePeriod);
	}
}