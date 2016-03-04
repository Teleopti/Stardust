using System;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestDetailViewModelFactory
	{
		IAbsenceRequestDetailViewModel CreateAbsenceRequestDetailViewModel(Guid absenceRequestId);
	}
}