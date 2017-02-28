using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestDetailViewModelFactory
	{
		IAbsenceRequestDetailViewModel CreateAbsenceRequestDetailViewModel(Guid absenceRequestId);
	}
}