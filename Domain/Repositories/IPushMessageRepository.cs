using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPushMessagePersister
	{
		ISendPushMessageReceipt Add(IPushMessage pushMessage, IEnumerable<IPerson> receivers);
		void Remove(IPushMessage pushMessage);
	}

	public interface IPushMessageRepository:IRepository<IPushMessage>
	{

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
	}
}
