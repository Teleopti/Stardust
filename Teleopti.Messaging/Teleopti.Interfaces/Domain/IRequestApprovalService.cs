using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Service to help approving requests
	/// </summary>
	/// <remarks>
	/// Created by: robink
	/// Created date: 2008-10-10
	/// </remarks>
	public interface IRequestApprovalService
	{
		/// <summary>
		/// Approves the absence.
		/// </summary>
		/// <param name="absence">The absence.</param>
		/// <param name="period">The period.</param>
		/// <param name="person">The person.</param>
		/// <returns>any rules broken</returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-10-10
		/// </remarks>
		IEnumerable<IBusinessRuleResponse> ApproveAbsence(IAbsence absence, DateTimePeriod period, IPerson person, IAbsenceRequest absenceRequest = null);

		/// <summary>
		/// Approves the shift trade.
		/// </summary>
		/// <param name="shiftTradeRequest">The shift trade request.</param>
		/// <returns>any rules broken</returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-11-04
		/// </remarks>
		IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest);
	}
}