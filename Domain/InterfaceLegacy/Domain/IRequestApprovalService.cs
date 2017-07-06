using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
		/// Approves the shift trade.
		/// </summary>
		/// <param name="shiftTradeRequest">The shift trade request.</param>
		/// <returns>any rules broken</returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-11-04
		/// </remarks>
		IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest);

		IEnumerable<IBusinessRuleResponse> Approve(IPersonRequest personRequest);
	}
}