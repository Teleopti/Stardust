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

		public void SetApprovalService(IRequestApprovalService requestApprovalService)
		{
			_approvalService = requestApprovalService;
		}

		public IRequestApprovalService MakeAbsenceRequestApprovalService(IScheduleDictionary scheduleDictionary, IScenario scenario,
			IPerson person)
		{
			return _approvalService ?? (_approvalService = MockRepository.GenerateMock<AbsenceRequestApprovalService, IRequestApprovalService, IAbsenceApprovalService>(scenario, scheduleDictionary, null, null, null, null));
		}

		public IRequestApprovalService MakeShiftTradeRequestApprovalService(IScheduleDictionary scheduleDictionary,
			IPerson person)
		{
			return _approvalService ??
				   (_approvalService =
					   MockRepository.GenerateMock<ShiftTradeRequestApprovalService, IRequestApprovalService>(null, null, null, null));
		}

		public IRequestApprovalService MakeOvertimeRequestApprovalService(ISkill[] validatedSkills)
		{
			return _approvalService ??
				   (_approvalService =
					   MockRepository.GenerateMock<OvertimeRequestApprovalService, IRequestApprovalService>(null, null, null,
						   validatedSkills));
		}
	}
}
