using System.Collections.Generic;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class ApproveAbsenceRequestWithValidators : ProcessAbsenceRequest
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ApproveAbsenceRequestWithValidators));
		public override void Process(IPerson processingPerson, IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			foreach (var absenceRequestValidator in absenceRequestValidatorList)
			{
				IValidatedRequest validatedRequest = absenceRequestValidator.Validate(absenceRequest,
					requiredForHandlingAbsenceRequest);

				if (!validatedRequest.IsValid)
				{
					if (Logger.IsWarnEnabled)
						Logger.WarnFormat("one validator {0} failed",
							absenceRequestValidator);
					return;
				}
			}

			UndoAll(requiredForProcessingAbsenceRequest);
			requiredForProcessingAbsenceRequest.AfterUndoCallback();

			IPersonRequest personRequest = (IPersonRequest)absenceRequest.Parent;
			personRequest.Pending();
			var result = personRequest.Approve(requiredForProcessingAbsenceRequest.RequestApprovalService,
				requiredForProcessingAbsenceRequest.Authorization);
			foreach (IBusinessRuleResponse ruleResponse in result)
			{
				if (Logger.IsWarnEnabled)
					Logger.WarnFormat("At least one validation rule failed, the schedule cannot be changed. The error was: {0}",
						ruleResponse.Message);
			}

		}

		public override string DisplayText
		{
			get { return UserTexts.Resources.Yes; }
		}
		public override IProcessAbsenceRequest CreateInstance()
		{
			return new ApproveAbsenceRequestWithValidators();
		}
	}
}
