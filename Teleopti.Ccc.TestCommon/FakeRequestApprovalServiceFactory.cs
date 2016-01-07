using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRequestApprovalServiceFactory : IRequestApprovalServiceFactory
	{
		private IRequestApprovalService _approvalService;

		public IRequestApprovalService MakeRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary, IScenario scenario)
		{
			return _approvalService ?? (_approvalService = MockRepository.GenerateMock<IRequestApprovalService>());
		}

		public void Reset()
		{
			_approvalService = null;
		}
	}
}
