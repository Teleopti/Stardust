using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PushMessageDialogueRepository : Repository<IPushMessageDialogue>, IPushMessageDialogueRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PushMessageRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public PushMessageDialogueRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public PushMessageDialogueRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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
             .SetFetchMode("DialogueMessages", FetchMode.Join) 
             .SetFetchMode("Receiver", FetchMode.Join)
             .SetResultTransformer(Transformers.DistinctRootEntity) 
             .Add(Restrictions.Eq("PushMessage", pushMessage))
             .List<IPushMessageDialogue>();
        }

        public IList<IPushMessageDialogue> FindAllPersonMessagesNotRepliedTo(IPerson person)
        {
            return Session.CreateCriteria(typeof(PushMessageDialogue), "dialogue")
             .SetFetchMode("DialogueMessages", FetchMode.Join)
             .SetFetchMode("Receiver", FetchMode.Join)
             .SetResultTransformer(Transformers.DistinctRootEntity)
             .Add(Restrictions.Eq("Receiver", person))
             .Add(Restrictions.Eq("IsReplied", false))
             .List<IPushMessageDialogue>();
        }

        public void Remove(IPushMessage pushMessage)
        {
            Find(pushMessage).ForEach(Remove);
        }
    }
}
