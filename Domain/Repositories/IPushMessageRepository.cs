using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPushMessageRepository:IRepository<IPushMessage>
	{
		/// <summary>
		/// Adds the specified pushMessage and creates the PushMessageDialogues for the receivers
		/// </summary>
		/// <param name="pushMessage">The conversation.</param>
		/// <param name="receivers">The receivers.</param>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-05-19
		/// </remarks>
		void Add(IPushMessage pushMessage, IEnumerable<IPerson> receivers);


		/// <summary>
		/// Adds the specified pushMessage and creates the PushMessageDialogues for the receivers
		/// </summary>
		/// <param name="pushMessage">The conversation.</param>
		/// <param name="receivers">The receivers.</param>
		/// <param name="createPushMessageDialoguesService">Service for creating dialogues</param>
		/// <returns>
		/// Information about roots that was created
		/// </returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-10-22
		/// </remarks>
		ISendPushMessageReceipt Add(IPushMessage pushMessage, IEnumerable<IPerson> receivers,
									ICreatePushMessageDialoguesService createPushMessageDialoguesService);

		/// <summary>
		/// Finds the messages sent from sender.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="pagingDetail">The details of paging.</param>
		/// <returns></returns>
		/// <remarks>
		/// If sender is not set, it will look at the created by
		/// </remarks>
		ICollection<IPushMessage> Find(IPerson sender,PagingDetail pagingDetail);

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

		ICollection<IPushMessageDialogue> FindUnreadMessage(Paging paging, IPerson receiver);	
	}
}
