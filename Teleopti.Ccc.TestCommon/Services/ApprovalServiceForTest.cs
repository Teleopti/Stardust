using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Services
{
	public class ApprovalServiceForTest : IRequestApprovalService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IScenario Scenario
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IBusinessRuleResponse> ApproveAbsence(IAbsence absence, DateTimePeriod period, IPerson person, IAbsenceRequest absenceRequest)
		{
			return new List<IBusinessRuleResponse>();
		}

		public IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest)
		{
			return new List<IBusinessRuleResponse>();
		}
	}
}