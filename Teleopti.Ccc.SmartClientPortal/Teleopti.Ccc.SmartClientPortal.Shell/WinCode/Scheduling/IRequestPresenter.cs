using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IRequestPresenter : IRequestPresenterCallback
    {
        /// <summary>
        /// Approves the or deny.
        /// </summary>
        /// <param name="personRequestViewModels"></param>
        /// <param name="command">The command.</param>
        /// <param name="replyText"></param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-08
        /// </remarks>
        void ApproveOrDeny(IList<PersonRequestViewModel> personRequestViewModels, IHandlePersonRequestCommand command, string replyText);

        /// <summary>
        /// Replies the specified request view adaptors.
        /// </summary>
        /// <param name="personRequestViewModels"></param>
        /// <param name="replyText">The reply text.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-06-15
        /// </remarks>
        void Reply(IList<PersonRequestViewModel> personRequestViewModels, string replyText);

        /// <summary>
        /// Sets the undo redo container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-06-16
        /// </remarks>
        void SetUndoRedoContainer(IUndoRedoContainer container);
    }
}