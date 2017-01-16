﻿using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRequestApprovalServiceFactory : IRequestApprovalServiceFactory
	{
		private IRequestApprovalService _approvalService;

		public void Reset()
		{
			_approvalService = null;
		}

		public IRequestApprovalService MakeRequestApprovalServiceScheduler (IScheduleDictionary scheduleDictionary, IScenario scenario,
			IPerson person)
		{
			return _approvalService ?? (_approvalService = MockRepository.GenerateMock<IRequestApprovalService>());
		}
	}
}
