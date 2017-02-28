using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class PendingAbsenceRequest : ProcessAbsenceRequest
	{
		public override string DisplayText
		{
			get { return UserTexts.Resources.No; }
		}

		public override IProcessAbsenceRequest CreateInstance()
		{
			return new PendingAbsenceRequest();
		}

		public override void Process(IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			if (
				!CheckValidatorList(absenceRequest, requiredForProcessingAbsenceRequest,
					requiredForHandlingAbsenceRequest, absenceRequestValidatorList)) return;

			UndoAll(requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, absenceRequest);

			IPersonRequest personRequest = (IPersonRequest)absenceRequest.Parent;
			personRequest.Pending();
		}

		public override bool Equals(object obj)
		{
			PendingAbsenceRequest process = obj as PendingAbsenceRequest;
			return process != null;
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