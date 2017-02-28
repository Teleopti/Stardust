using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics
{
	public interface IAnalyticsAbsence
	{
		int AbsenceId { get; set; }
		Guid AbsenceCode { get; set; }
		bool InPaidTime { get; set; }
	}
}