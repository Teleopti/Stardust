using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestApprovalService : IRequestApprovalService
	{
		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			var overtimeRequest = request as IOvertimeRequest;
			if (overtimeRequest == null)
			{
				throw new InvalidCastException("Request type should be OvertimeRequest!");
			}

			var person = overtimeRequest.Person;
			var period = overtimeRequest.Period;
			var overtime = overtimeRequest.MultiplicatorDefinitionSet;

			return new List<IBusinessRuleResponse>();
		}
	}
}
