using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IReassociateDataForSchedules
	{
		void ReassociateDataForAllPeople();
		//todo - start using this one!
		void ReassociateDataFor(IPerson person);
	}
}