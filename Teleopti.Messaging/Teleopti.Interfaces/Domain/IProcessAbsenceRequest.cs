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
        /// <param name="authorization">The authorization checker.</param>
        /// <param name="absenceRequestValidatorList">The absence request validator list.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-21
        /// </remarks>
        void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, IPersonRequestCheckAuthorization authorization, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList);

        /// <summary>
        /// Gets or sets the request approval service.
        /// </summary>
        /// <value>The request approval service.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-21
        /// </remarks>
        IRequestApprovalService RequestApprovalService { get; set; }

        /// <summary>
        /// Gets or sets the undo redo container. Mainly used to do a rollback on the simulated approval.
        /// </summary>
        /// <value>The undo redo container.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-22
        /// </remarks>
        IUndoRedoContainer UndoRedoContainer { get; set; }

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
}