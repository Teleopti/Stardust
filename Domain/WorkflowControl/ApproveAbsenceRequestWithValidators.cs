using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class ApproveAbsenceRequestWithValidators : ProcessAbsenceRequest
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ApproveAbsenceRequestWithValidators));

		public override void Process(IPerson processingPerson, IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			foreach (var absenceRequestValidator in absenceRequestValidatorList)
			{
				var validatedRequest = absenceRequestValidator.Validate(absenceRequest,
					requiredForHandlingAbsenceRequest);

				if (validatedRequest.IsValid) continue;

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Validator {0} failed for request with Id=\"{1}\"",
						absenceRequestValidator, absenceRequest.Id);
				}
				return;
			}

			UndoAll(requiredForProcessingAbsenceRequest);
			requiredForProcessingAbsenceRequest.AfterUndoCallback();

			var personRequest = (IPersonRequest)absenceRequest.Parent;
			personRequest.Pending();

			var result = personRequest.Approve(requiredForProcessingAbsenceRequest.RequestApprovalService,
				requiredForProcessingAbsenceRequest.Authorization);

			if (!result.Any()) return;

			if (logger.IsDebugEnabled)
			{
				var errorMessageBuilder = new StringBuilder();
				foreach (var ruleResponse in result)
				{
					errorMessageBuilder.AppendLine(ruleResponse.Message);
				}
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
	}
}
