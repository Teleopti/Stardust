using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PushMessageRepository :Repository<IPushMessage>,IPushMessageRepository
    {
        private IPushMessageDialogueRepository _pushMessageDialogueRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushMessageRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public PushMessageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _pushMessageDialogueRepository = new PushMessageDialogueRepository(unitOfWork);
        }

        public PushMessageRepository(IUnitOfWorkFactory unitOfWorkFactory, IPushMessageDialogueRepository pushMessageDialogueRepository)
            : base(unitOfWorkFactory)
        {
            _pushMessageDialogueRepository = pushMessageDialogueRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PushMessageRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="pushMessageDialogueRepository">The conversation dialogue repository.</param>
        /// <remarks>
        /// Injects a repository for ConversationDialogues
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        public PushMessageRepository(IUnitOfWork unitOfWork,IPushMessageDialogueRepository pushMessageDialogueRepository) : base(unitOfWork)
        {
            _pushMessageDialogueRepository = pushMessageDialogueRepository;
        }

        /// <summary>
        /// Removes the specified entity from repository.
        /// Will be deleted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <remarks>
        /// Removes any ConversationDialogues as well
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        public override void Remove(IPushMessage entity)
        {
            _pushMessageDialogueRepository.Remove(entity);
            base.Remove(entity);
        }

        /// <summary>
        /// Adds the specified pushMessage and creates the dialogues
        /// </summary>
        /// <param name="pushMessage">The conversation.</param>
        /// <param name="receivers">The receivers.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-05-19
        /// </remarks>
        public void Add(IPushMessage pushMessage, IEnumerable<IPerson> receivers)
        {
            Add(pushMessage,receivers,new CreatePushMessageDialoguesService());
        }

        public ISendPushMessageReceipt Add(IPushMessage pushMessage,IEnumerable<IPerson> receivers,ICreatePushMessageDialoguesService createPushMessageDialoguesService)
        {
            Add(pushMessage);
            ISendPushMessageReceipt receipt = createPushMessageDialoguesService.Create(pushMessage, receivers);
            receipt.CreatedDialogues.ForEach(d => _pushMessageDialogueRepository.Add(d));
            return receipt;
        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public ICollection<IPushMessage> Find(IPerson sender, PagingDetail pagingDetail)
        {
			// Get the total row count in the database. 
			var rowCount = Session.CreateCriteria(typeof(PushMessage))
				.Add(Restrictions.Disjunction()
						.Add(Restrictions.Eq("Sender", sender))
						.Add(Restrictions.Eq("CreatedBy", sender) && Restrictions.IsNull("Sender")))
				.SetProjection(Projections.RowCount())
				.FutureValue<int>();

			// Get the actual log entries, respecting the paging. 
        	var results = Session.CreateCriteria(typeof (PushMessage))
        		.Add(Restrictions.Disjunction()
        		     	.Add(Restrictions.Eq("Sender", sender))
        		     	.Add(Restrictions.Eq("CreatedBy", sender) && Restrictions.IsNull("Sender")))
        		.SetFetchMode("ReplyOptions", FetchMode.Lazy)
        		.SetFirstResult(pagingDetail.Skip)
        		.SetMaxResults(pagingDetail.Take)
        		.Future<IPushMessage>();

        	pagingDetail.TotalNumberOfResults = rowCount.Value;

        	return results.ToList();
        }
    }
}
