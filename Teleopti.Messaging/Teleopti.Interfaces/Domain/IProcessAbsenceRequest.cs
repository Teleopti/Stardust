using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Process an absence request by Granting or setting to Pending
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2010-04-21
    /// </remarks>
    public interface IProcessAbsenceRequest
    {
        /// <summary>
        /// Processes the specified absence request.
        /// </summary>
        /// <param name="processingPerson">The processing person.</param>
        /// <param name="absenceRequest">The absence request.</param>
        /// <param name="requiredForProcessingAbsenceRequest">The stuff required for processing an absence request.</param>
        /// <param name="requiredForHandlingAbsenceRequest"></param>
        /// <param name="absenceRequestValidatorList">The absence request validator list.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-21
        /// </remarks>
        void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList);

        /// <summary>
        /// Gets the display text.
        /// </summary>
        /// <value>The display text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-27
        /// </remarks>
        string DisplayText { get; }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-21
        /// </remarks>
        IProcessAbsenceRequest CreateInstance();
    }

    /// <summary>
    /// 
    /// </summary>
    public struct RequiredForProcessingAbsenceRequest
    {
        private readonly IUndoRedoContainer _undoRedoContainer;
        private readonly IRequestApprovalService _requestApprovalService;
        private readonly IPersonRequestCheckAuthorization _authorization;
	    private Action _afterUndoCallback;

	    /// <summary>
        /// 
        /// </summary>
        /// <param name="undoRedoContainer"></param>
        /// <param name="requestApprovalService"></param>
        /// <param name="authorization"></param>
        public RequiredForProcessingAbsenceRequest(IUndoRedoContainer undoRedoContainer, IRequestApprovalService requestApprovalService, IPersonRequestCheckAuthorization authorization, Action afterUndoCallback = null)
        {
            _undoRedoContainer = undoRedoContainer;
            _requestApprovalService = requestApprovalService;
            _authorization = authorization;
	        _afterUndoCallback = afterUndoCallback ?? (()=>{});
        }

        /// <summary>
        /// 
        /// </summary>
        public IRequestApprovalService RequestApprovalService
        {
            get { return _requestApprovalService; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IUndoRedoContainer UndoRedoContainer
        {
            get { return _undoRedoContainer; }
        }

        /// <summary>
        /// 
        /// </summary>
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