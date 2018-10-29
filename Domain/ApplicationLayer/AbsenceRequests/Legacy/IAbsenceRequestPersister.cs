using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy
{
	public interface IAbsenceRequestPersister
	{
		IPersonRequest Persist(AbsenceRequestModel model);
	}
}