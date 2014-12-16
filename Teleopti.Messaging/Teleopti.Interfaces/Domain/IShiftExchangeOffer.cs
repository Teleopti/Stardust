namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// shift exchange offer from person
	/// </summary>
	/// <remarks>
	/// Created by: mingdi
	/// Created date: 2014-12-12
	/// </remarks>
	public interface IShiftExchangeOffer : IAggregateRoot, IChangeInfo
	{
		DateOnly Date { get; }

		DateTimePeriod? MyShiftPeriod { get; }

		IPerson Person { get; }

		long Checksum { get; }

		ShiftExchangeOfferStatus Status { get; set; }

		bool IsWantedSchedule(IScheduleDay scheduleToCheck);

		IPersonRequest MakeShiftTradeRequest(IScheduleDay scheduleToTrade);
	}
}