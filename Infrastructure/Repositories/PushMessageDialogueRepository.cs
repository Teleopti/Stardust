using System.Collections.Generic;
using System.Collections.ObjectModel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PushMessageDialogueRepository : Repository<IPushMessageDialogue>, IPushMessageDialogueRepository
	{
		public PushMessageDialogueRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public PushMessageDialogueRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
			
		}

		/// <summary>
		/// Finds the the dialogues that belongs to the pushMessage.
		/// </summary>
		/// <param name="pushMessage">The conversation.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-05-18
		/// </remarks>
		public IList<IPushMessageDialogue> Find(IPushMessage pushMessage)
		{
			return Session.CreateCriteria(typeof(PushMessageDialogue), "dialogue")
			 .Fetch("DialogueMessages") 
			 .Fetch("Receiver")
			 .SetResultTransformer(Transformers.DistinctRootEntity) 
			 .Add(Restrictions.Eq("PushMessage", pushMessage))
			 .List<IPushMessageDialogue>();
		}

		public IList<IPushMessageDialogue> FindAllPersonMessagesNotRepliedTo(IPerson person)
		{
			return Session.CreateCriteria(typeof(PushMessageDialogue), "dialogue")
			 .Fetch("DialogueMessages")
			 .Fetch("Receiver")
			 .SetResultTransformer(Transformers.DistinctRootEntity)
			 .Add(Restrictions.Eq("Receiver", person))
			 .Add(Restrictions.Eq("IsReplied", false))
			 .List<IPushMessageDialogue>();
		}

		public void Remove(IPushMessage pushMessage)
		{
			Find(pushMessage).ForEach(Remove);
		}

		public int CountUnread(IPerson receiver)
		{
			var rowCount = Session.CreateCriteria(typeof(PushMessageDialogue))
				.Add(Restrictions.Eq("Receiver", receiver))
				.Add(Restrictions.Eq("IsReplied", false))
				.SetProjection(Projections.RowCount())
				.FutureValue<int>();

			return rowCount.Value;
		}

		public ICollection<IPushMessageDialogue> FindUnreadMessages(Paging paging, IPerson receiver)
		{
			var criteria = Session.CreateCriteria(typeof(PushMessageDialogue))
				.Add(Restrictions.Eq("Receiver", receiver))
				.Add(Restrictions.Eq("IsReplied", false))
				.AddOrder(Order.Desc("UpdatedOn"));

			if (!paging.Equals(Paging.Empty))
			{
				criteria
					.SetFirstResult(paging.Skip)
					.SetMaxResults(paging.Take);
			}

			return new Collection<IPushMessageDialogue>(criteria.List<IPushMessageDialogue>());
		}
	}
}
