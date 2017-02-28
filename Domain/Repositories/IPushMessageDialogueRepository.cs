using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		/// <summary>
		/// Counts the number of unread messags for the given receiver.
		/// </summary>
		/// <param name="receiver">The receiver that unread messages is counted for.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2012-09-21
		/// </remarks>
		int CountUnread(IPerson receiver);

		/// <summary>
		/// Finds all unread messages for the given receiver.
		/// </summary>
		/// <param name="paging">paging object tells how many items to get.</param>
		/// <param name="receiver">The receiver that unread messages is fetched for.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2012-10-01
		/// </remarks>
		ICollection<IPushMessageDialogue> FindUnreadMessages(Paging paging, IPerson receiver);
	}
}
