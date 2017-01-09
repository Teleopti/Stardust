using System.Collections.Generic;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	/// <summary>
	/// Stub so we dont have to mock validators, gets a little bit easier to follow
	/// </summary>
	internal class ValidatorSpecificationForTest : ShiftTradeSpecification
	{
		private readonly bool _isSatisfiedBy;
		private IEnumerable<IShiftTradeSwapDetail> _calledWith;

		public override string DenyReason { get; }

		public bool HasBeenCalledWith(IEnumerable<IShiftTradeSwapDetail> details)
		{
			return details.Equals(_calledWith);
		}

		public ValidatorSpecificationForTest(bool isSatisfiedBy, string denyReason)
		{
			_isSatisfiedBy = isSatisfiedBy;
			DenyReason = denyReason;
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			_calledWith = obj;

			return _isSatisfiedBy;
		}
	}

	internal class DummySpecification : ShiftTradeSpecification
	{
		private readonly bool _isSatisfied;
		public bool WasCalled { get; private set; }
		private IEnumerable<IShiftTradeSwapDetail> _calledWith;

		public DummySpecification(bool isSatisfied, string denyReason = "Deny reason for DummySpecification")
		{
			_isSatisfied = isSatisfied;
			DenyReason = denyReason;
		}

		public bool HasBeenCalledWith(IEnumerable<IShiftTradeSwapDetail> swapDetails)
		{
			return swapDetails.Equals(_calledWith);
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> swapDetail)
		{
			WasCalled = true;
			_calledWith = swapDetail;
			return _isSatisfied;
		}

		public override string DenyReason { get; }
	}
}