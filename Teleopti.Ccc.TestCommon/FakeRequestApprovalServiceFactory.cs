using System;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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

		public IRequestApprovalService MakeAbsenceRequestApprovalService(IScheduleDictionary scheduleDictionary, IScenario scenario,
			IPersonRequest personRequest)
		{
			return _approvalService ?? (_approvalService = MockRepository.GenerateMock<AbsenceRequestApprovalService, IRequestApprovalService, IAbsenceRequestApprovalService>(scenario, scheduleDictionary,null,null,null,null));
		}
	}
}
