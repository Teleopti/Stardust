using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IProcessAbsenceRequest
	{
		void Process(IPerson processingPerson, IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList);

		string DisplayText { get; }
		IProcessAbsenceRequest CreateInstance();
	}

	public struct RequiredForProcessingAbsenceRequest
	{
		public RequiredForProcessingAbsenceRequest(IUndoRedoContainer undoRedoContainer,
			IRequestApprovalService requestApprovalService, IPersonRequestCheckAuthorization authorization,
			Action afterUndoCallback = null)
		{
			UndoRedoContainer = undoRedoContainer;
			RequestApprovalService = requestApprovalService;
			Authorization = authorization;
			AfterUndoCallback = afterUndoCallback ?? (() => { });
		}

		public IRequestApprovalService RequestApprovalService { get; }

		public IUndoRedoContainer UndoRedoContainer { get; }

		public IPersonRequestCheckAuthorization Authorization { get; }

		public Action AfterUndoCallback { get; set; }
	}
}