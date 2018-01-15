using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class ApproveAbsenceRequestWithValidators : ProcessAbsenceRequest
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ApproveAbsenceRequestWithValidators));

		public override void Process(IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			foreach (var absenceRequestValidator in absenceRequestValidatorList)
			{
				var validatedRequest = absenceRequestValidator.Validate(absenceRequest,
					requiredForHandlingAbsenceRequest);

				if (validatedRequest.IsValid) continue;

				appendErrorMessage(absenceRequest, validatedRequest.ValidationErrors);

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Validator {0} failed for request with Id=\"{1}\"",
						absenceRequestValidator, absenceRequest.Id);
				}
				return;
			}

			UndoAll(requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, absenceRequest);
			requiredForProcessingAbsenceRequest.AfterUndoCallback();

			var personRequest = (IPersonRequest)absenceRequest.Parent;
			personRequest.Pending();

			var result = personRequest.Approve(requiredForProcessingAbsenceRequest.RequestApprovalService,
				requiredForProcessingAbsenceRequest.Authorization);

			if (!result.Any()) return;

			if (logger.IsDebugEnabled)
			{
				var errorMessageBuilder = string.Join(Environment.NewLine, result.Select(ruleResponse => ruleResponse.Message));
				logger.DebugFormat("{0} validation rule(s) failed on approving request with Id=\"{1}\", "
								   + "the schedule cannot be changed. Below is the error messages: \r\n{2}",
					result.Count, absenceRequest.Id, errorMessageBuilder);
			}
		}

		public override string DisplayText => UserTexts.Resources.Yes;

		public override IProcessAbsenceRequest CreateInstance()
		{
			return new ApproveAbsenceRequestWithValidators();
		}

		private void appendErrorMessage(IAbsenceRequest absenceRequest, string errorMessage)
		{
			var personRequest = (IPersonRequest) absenceRequest.Parent;
			var originalMessage = personRequest.GetMessage(new NoFormatting());
			if (string.IsNullOrWhiteSpace(originalMessage))
			{
				personRequest?.TrySetMessage(errorMessage);
			}
			else if (!originalMessage.Contains(errorMessage))
			{
				personRequest?.TrySetMessage(originalMessage.TrimEnd() + Environment.NewLine + errorMessage);
			}
		}
	}
}
