namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public enum RequestType
	{
		TextRequest,
		AbsenceRequest,
		ShiftTradeRequest,
		ShiftExchangeOffer,
		OvertimeRequest
	}

	public enum RequestStatus
	{
		New,
		Pending,
		Approved,
		Denied,
		Referred
	}
}