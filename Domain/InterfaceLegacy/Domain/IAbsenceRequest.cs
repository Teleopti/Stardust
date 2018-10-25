using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequest : IRequest
	{
		IAbsence Absence { get; }

		bool FullDay { get; set; }

		bool IsRequestForOneLocalDay (TimeZoneInfo timeZone);
	}
}