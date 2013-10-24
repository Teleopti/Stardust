using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IReassociateDataForSchedules
	{
		void ReassociateDataForAllPeople();
		void ReassociateDataFor(IPerson person);
	}
}