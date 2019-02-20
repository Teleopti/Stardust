using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	
	public class PushMessagePersister : IPushMessagePersister
	{
		private readonly IPushMessageRepository _repository;
		private readonly IPushMessageDialogueRepository _dialogueRepository;
		private readonly ICreatePushMessageDialoguesService _dialoguesService;

		public PushMessagePersister(IPushMessageRepository repository, IPushMessageDialogueRepository dialogueRepository, ICreatePushMessageDialoguesService dialoguesService)
		{
			_repository = repository;
			_dialogueRepository = dialogueRepository;
			_dialoguesService = dialoguesService;
		}

		public ISendPushMessageReceipt Add(IPushMessage pushMessage, IEnumerable<IPerson> receivers)
		{
			_repository.Add(pushMessage);
			ISendPushMessageReceipt receipt = _dialoguesService.Create(pushMessage, receivers);
			receipt.CreatedDialogues.ForEach(d => _dialogueRepository.Add(d));
			return receipt;
		}

		public void Remove(IPushMessage pushMessage)
		{
			_repository.Remove(pushMessage);
			_dialogueRepository.Remove(pushMessage);
		}

	}

	public class PushMessageRepository :Repository<IPushMessage>, IPushMessageRepository
	{

#pragma warning disable 618
		public PushMessageRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public PushMessageRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
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
				.Fetch(SelectMode.Skip, "ReplyOptions")
				.SetFirstResult(pagingDetail.Skip)
				.SetMaxResults(pagingDetail.Take)
				.Future<IPushMessage>();

			pagingDetail.TotalNumberOfResults = rowCount.Value;

			return results.ToList();
		}
	}
}
