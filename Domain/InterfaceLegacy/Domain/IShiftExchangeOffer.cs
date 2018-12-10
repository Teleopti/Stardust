namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// shift exchange offer from person
	/// </summary>
	/// <remarks>
	/// Created by: mingdi
	/// Created date: 2014-12-12
	/// </remarks>
	public interface IShiftExchangeOffer : IRequest
	{
		DateOnly Date { get; set; }

		DateOnly ValidTo { get; set; }

		DateTimePeriod? MyShiftPeriod { get; set; }

		long Checksum { get; set; }

		ShiftExchangeOfferStatus Status { get; set; }

		bool IsWantedSchedule(IScheduleDay scheduleToCheck);

		IPersonRequest MakeShiftTradeRequest(IScheduleDay scheduleToTrade);

		string GetStatusText();

		bool IsExpired();

		string ShiftExchangeOfferId { get; set; }
		ShiftExchangeLookingForDay DayType { get; set; }
		IShiftExchangeCriteria Criteria { get; set; }
	}
}