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
        private readonly IUndoRedoContainer _undoRedoContainer;
        private readonly IRequestApprovalService _requestApprovalService;
        private readonly IPersonRequestCheckAuthorization _authorization;
	    private Action _afterUndoCallback;

	    public RequiredForProcessingAbsenceRequest(IUndoRedoContainer undoRedoContainer,
		    IRequestApprovalService requestApprovalService, IPersonRequestCheckAuthorization authorization,
		    Action afterUndoCallback = null)
	    {
		    _undoRedoContainer = undoRedoContainer;
		    _requestApprovalService = requestApprovalService;
		    _authorization = authorization;
		    _afterUndoCallback = afterUndoCallback ?? (() => { });
	    }

        public IRequestApprovalService RequestApprovalService
        {
            get { return _requestApprovalService; }
        }

        public IUndoRedoContainer UndoRedoContainer
        {
            get { return _undoRedoContainer; }
        }

        public IPersonRequestCheckAuthorization Authorization
        {
            get { return _authorization; }
        }

	    public Action AfterUndoCallback
	    {
		    get { return _afterUndoCallback; }
			set { _afterUndoCallback = value; }
	    }
    }
}