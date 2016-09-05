using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Services
{
	public class ApprovalServiceForTest : IRequestApprovalService
	{

	    private IEnumerable<IBusinessRuleResponse> businessRuleResponse = new List<IBusinessRuleResponse>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IScenario Scenario
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IBusinessRuleResponse> ApproveAbsence (IAbsence absence, DateTimePeriod period, IPerson person, IPersonRequest personRequest = null)
		{
		    return businessRuleResponse;

		}

		public IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest)
		{
		    return businessRuleResponse;

		}

		public IPersonAbsence GetApprovedPersonAbsence()
		{
			return null;
		}

		public void SetBusinessRuleResponse(params IBusinessRuleResponse[] brokenRule)
	    {
	        businessRuleResponse = brokenRule;
	    }
	}
}