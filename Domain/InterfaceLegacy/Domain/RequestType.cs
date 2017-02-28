namespace Teleopti.Interfaces.Domain
{
	public enum RequestType
	{
		TextRequest,
		AbsenceRequest,
		ShiftTradeRequest,
		ShiftExchangeOffer
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