using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRequestApprovalServiceFactory : IRequestApprovalServiceFactory
	{
		private IRequestApprovalService _approvalService;

		public void Reset()
		{
			_approvalService = null;
		}

		public IRequestApprovalService MakeRequestApprovalService(IScheduleDictionary scheduleDictionary, IScenario scenario, IPersonRequest personRequest)
		{
			return _approvalService ?? (_approvalService = MockRepository.GenerateMock<AbsenceRequestApprovalService, IRequestApprovalService, IAbsenceApprovalService>(scenario, scheduleDictionary,null,null,null,null));
		}
	}
}
