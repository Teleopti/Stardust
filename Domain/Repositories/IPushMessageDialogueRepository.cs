using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
   
    public interface IPushMessageDialogueRepository : IRepository<IPushMessageDialogue>
    {
        /// <summary>
        /// Finds the the dialogues that belongs to the pushMessage.
        /// </summary>
        /// <param name="pushMessage">The conversation.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-18
        /// </remarks>
        IList<IPushMessageDialogue> Find(IPushMessage pushMessage);

        /// <summary>
        /// Finds all person messages unreplied.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-05-27
        /// </remarks>
        IList<IPushMessageDialogue> FindAllPersonMessagesNotRepliedTo(IPerson person);

        /// <summary>
        /// Removes all ConversationDialogues belonging to a pushMessage
        /// </summary>
        /// <param name="pushMessage">The conversation.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        void Remove(IPushMessage pushMessage);

    }
}
