using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class DenyAbsenceRequest : ProcessAbsenceRequest
	{
		public DenyAbsenceRequest()
		{
			DenyReason = nameof(Resources.RequestDenyReasonAutodeny);
		}

		public override string DisplayText
		{
			get { return UserTexts.Resources.Deny; }
		}

		public string DenyReason { get; set; }

		public PersonRequestDenyOption? DenyOption { get; set; }

		public override IProcessAbsenceRequest CreateInstance()
		{
			return new DenyAbsenceRequest { DenyReason = DenyReason };
		}

		public override void Process(IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			UndoAll(requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, absenceRequest);
			var personRequest = (IPersonRequest) absenceRequest.Parent;
			var denyOption = PersonRequestDenyOption.AutoDeny | DenyOption.GetValueOrDefault(PersonRequestDenyOption.None);
			personRequest.Deny(DenyReason, requiredForProcessingAbsenceRequest.Authorization, absenceRequest.Person, denyOption);
		}

		public override bool Equals(object obj)
		{
			DenyAbsenceRequest process = obj as DenyAbsenceRequest;
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