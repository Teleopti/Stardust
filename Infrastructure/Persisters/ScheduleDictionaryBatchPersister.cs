using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDictionaryBatchPersister : IScheduleDictionaryBatchPersister
	{
		private readonly ICurrentUnitOfWorkFactory _uowFactory;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;
		private readonly IReassociateData _reassociateData;
		private readonly IScheduleDictionaryModifiedCallback _callback;

		public ScheduleDictionaryBatchPersister(ICurrentUnitOfWorkFactory uowFactory, IScheduleRepository scheduleRepository, IScheduleDictionarySaver scheduleDictionarySaver, IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, IMessageBrokerIdentifier messageBrokerIdentifier, IReassociateData reassociateData, IScheduleDictionaryModifiedCallback callback)
		{
			_uowFactory = uowFactory;
			_scheduleRepository = scheduleRepository;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_differenceCollectionService = differenceCollectionService;
			_messageBrokerIdentifier = messageBrokerIdentifier;
			_reassociateData = reassociateData;
			_callback = callback;
		}

		public void Persist(IScheduleDictionary scheduleDictionary)
		{
			var uowFactory = _uowFactory.LoggedOnUnitOfWorkFactory();
			foreach (var scheduleRange in scheduleDictionary.Values)
			{
				var diff = scheduleRange.DifferenceSinceSnapshot(_differenceCollectionService);
				if (diff.IsEmpty())
					continue;
				foreach (var item in diff)
				{
					using (var uow = makeUnitOfWork(uowFactory, scheduleRange))
					{
						var result = _scheduleDictionarySaver.MarkForPersist(uow, _scheduleRepository, item);
						uow.PersistAll(_messageBrokerIdentifier);
						if (_callback != null)
							_callback.Callback(scheduleRange, result.ModifiedEntities, result.AddedEntities, result.DeletedEntities);
					}
				}
				//behövs nog inte men för säkerhets skull...
				scheduleRange.TakeSnapshot();
			}
		}

		private IUnitOfWork makeUnitOfWork(IUnitOfWorkFactory unitOfWorkFactory, IScheduleRange scheduleRange)
		{
			return _reassociateData != null ? unitOfWorkFactory.CreateAndOpenUnitOfWork(_reassociateData.DataToReassociate(scheduleRange.Person)) :
																			unitOfWorkFactory.CreateAndOpenUnitOfWork();
		}
	}
}