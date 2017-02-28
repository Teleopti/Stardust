namespace Teleopti.Interfaces.Domain
{
	 /// <summary>
	 /// Status modes for Shift Exchange
	 /// </summary>
	 public enum ShiftExchangeOfferStatus
	 {
		 /// <summary>
		 /// Shift exchange offer has been send out waiting for shift trade
		 /// </summary>
		 Pending,

		 /// <summary>
		 /// Shift exchange offer has been took and made a shift trade successfully
		 /// </summary>
		 Completed,

		 /// <summary>
		 /// Shfit has been changed by admin after shift exchange offer send out
		 /// </summary>
		 Invalid,

		 /// <summary>
		 /// Shift trade from bulletin board is pending for admin approval
		 /// </summary>
		 PendingAdminApproval
	 };
}
