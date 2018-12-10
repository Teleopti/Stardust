using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class GrantAbsenceRequest : ProcessAbsenceRequest
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(GrantAbsenceRequest));

		public override string DisplayText => UserTexts.Resources.Yes;

		public override IProcessAbsenceRequest CreateInstance()
		{
			return new GrantAbsenceRequest();
		}

		public override void Process(IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			InParameter.NotNull(nameof(requiredForProcessingAbsenceRequest.RequestApprovalService), requiredForProcessingAbsenceRequest.RequestApprovalService);
			if (
				!CheckValidatorList(absenceRequest, requiredForProcessingAbsenceRequest,
					requiredForHandlingAbsenceRequest, absenceRequestValidatorList)) return;
			UndoAll(requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, absenceRequest);
			
			requiredForProcessingAbsenceRequest.AfterUndoCallback();

			IPersonRequest personRequest = (IPersonRequest)absenceRequest.Parent;
			personRequest.Pending();
			var result = personRequest.Approve(requiredForProcessingAbsenceRequest.RequestApprovalService,
				requiredForProcessingAbsenceRequest.Authorization);
			foreach (IBusinessRuleResponse ruleResponse in result)
			{
				if (logger.IsWarnEnabled)
					logger.WarnFormat("At least one validation rule failed, the schedule cannot be changed. The error was: {0}",
						ruleResponse.Message);
			}
		}

		public override bool Equals(object obj)
		{
			return obj is GrantAbsenceRequest;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				return result;
			}
		}
	}
}