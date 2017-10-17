using System;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	/// <summary>
	/// Result when validating a ShiftTradeRequest
	/// </summary>
	/// <remarks>
	/// Created by: henrika
	/// Created date: 2010-05-27
	/// </remarks>
	public class ShiftTradeRequestValidationResult
	{
		public ShiftTradeRequestValidationResult(bool shiftTradeIsOk) : this(shiftTradeIsOk, string.Empty)
		{
		}

		public ShiftTradeRequestValidationResult(bool shiftTradeIsOk, string denyReason)
			: this(shiftTradeIsOk, false, denyReason)
		{
		}

		public ShiftTradeRequestValidationResult(bool shiftTradeIsOk, bool shouldBeDenied, string denyReason)
		{
			IsOk = shiftTradeIsOk;
			ShouldBeDenied = shouldBeDenied;
			DenyReason = denyReason;
		}

		/// <summary>
		/// The result of validating a ShiftTradeRequest
		/// </summary>
		/// <value><c>true</c> if request passed all validation; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2010-05-27
		/// </remarks>
		public bool IsOk { get; private set; }

		/// <remarks>Indicate if this request should be denied if validation result is not ok.</remarks>
		/// <value><c>true</c> if this request should be denied; otherwise, <c>false</c>.</value>
		public bool ShouldBeDenied { get; private set; }

		/// <summary>
		/// Gets or sets the deny reason.
		/// </summary>
		/// <value>The deny reason.</value>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2010-05-27
		/// </remarks>
		public string DenyReason { get; private set; }

		public Type SpecificationType { get; set; }
	}
}