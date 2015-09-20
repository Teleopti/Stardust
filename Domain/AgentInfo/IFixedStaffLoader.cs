using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IFixedStaffLoader
	{
		PeopleSelection Load(DateOnlyPeriod period);
	}
}