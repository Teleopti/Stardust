using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.Services
{
	public class ApprovalServiceForTest : IRequestApprovalService, IAbsenceApprovalService
	{
		private IEnumerable<IBusinessRuleResponse> businessRuleResponse = new List<IBusinessRuleResponse>();

		public IScenario Scenario
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IAbsence absence, DateTimePeriod period, IPerson person)
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

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			return businessRuleResponse;
		}
	}
}