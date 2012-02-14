using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleScreenRetryingPersister : IScheduleScreenPersister
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IWriteProtectionRepository _writeProtectionRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPersonRequestPersister _personRequestPersister;
		private readonly IPersonAbsenceAccountConflictCollector _personAbsenceAccountConflictCollector;
		private readonly IPersonAbsenceAccountRefresher _personAbsenceAccountRefresher;
		private readonly IPersonAbsenceAccountValidator _personAbsenceAccountValidator;
		private readonly IScheduleDictionaryConflictCollector _scheduleDictionaryConflictCollector;
		private readonly IMessageBrokerModule _messageBrokerModule;
		private readonly IScheduleDictionaryBatchPersister _scheduleDictionaryBatchPersister;
		private readonly IOwnMessageQueue _messageQueueUpdater;

        public ScheduleScreenRetryingPersister(IUnitOfWorkFactory unitOfWorkFactory,  IWriteProtectionRepository writeProtectionRepository,
		                                       IPersonRequestRepository personRequestRepository,
		                                       IPersonAbsenceAccountRepository personAbsenceAccountRepository,
		                                       IPersonRequestPersister personRequestPersister,
		                                       IPersonAbsenceAccountConflictCollector personAbsenceAccountConflictCollector,
		                                       IPersonAbsenceAccountRefresher personAbsenceAccountRefresher,
		                                       IPersonAbsenceAccountValidator personAbsenceAccountValidator,
		                                       IScheduleDictionaryConflictCollector scheduleDictionaryConflictCollector,
		                                       IMessageBrokerModule messageBrokerModule,
												IScheduleDictionaryBatchPersister scheduleDictionaryBatchPersister,
												IOwnMessageQueue messageQueueUpdater)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
            _writeProtectionRepository = writeProtectionRepository;
			_personRequestRepository = personRequestRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_personRequestPersister = personRequestPersister;
			_personAbsenceAccountConflictCollector = personAbsenceAccountConflictCollector;
			_personAbsenceAccountRefresher = personAbsenceAccountRefresher;
			_personAbsenceAccountValidator = personAbsenceAccountValidator;
			_scheduleDictionaryConflictCollector = scheduleDictionaryConflictCollector;
			_messageBrokerModule = messageBrokerModule;
        	_scheduleDictionaryBatchPersister = scheduleDictionaryBatchPersister;
        	_messageQueueUpdater = messageQueueUpdater;
		}

		public IScheduleScreenPersisterResult TryPersist(IScheduleDictionary scheduleDictionary, 
															ICollection<IPersonWriteProtectionInfo> persons,
															IEnumerable<IPersonRequest> personRequests,
															ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			if(!persons.IsEmpty())
				saveWriteProtectionInSeparateTransaction(persons);
			bool retry;
			IEnumerable<IPersonAbsenceAccount> conflictedPersonAbsenceAccounts = null;
			do
			{
				try
				{
					transactions(conflictedPersonAbsenceAccounts, personAbsenceAccounts, scheduleDictionary, personRequests);
					retry = false;
				} 
				catch (OptimisticLockExceptionOnPersonAccount)
				{
					conflictedPersonAbsenceAccounts = GetPersonAbsenceAccountConflicts(personAbsenceAccounts);
					retry = true;
				} 
				catch (OptimisticLockExceptionOnScheduleDictionary)
				{
					var conflicts = GetScheduleDictionaryConflicts(scheduleDictionary);
					return new ScheduleScreenPersisterResult {Saved = false, ScheduleDictionaryConflicts = conflicts};
				} 
				catch (OptimisticLockException)
				{
					return new ScheduleScreenPersisterResult {Saved = false};
				}
			} while (retry);
			return new ScheduleScreenPersisterResult {Saved = true};
		}

		private void saveWriteProtectionInSeparateTransaction(ICollection<IPersonWriteProtectionInfo> personWriteProtectionInfos)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
                _writeProtectionRepository.AddRange(personWriteProtectionInfos);
				unitOfWork.PersistAll(_messageBrokerModule);
				personWriteProtectionInfos.Clear();
			}
		}

		private IEnumerable<IPersonAbsenceAccount> GetPersonAbsenceAccountConflicts(IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return _personAbsenceAccountConflictCollector.GetConflicts(uow, personAbsenceAccounts);
			}
		}

		private IEnumerable<IPersistConflict> GetScheduleDictionaryConflicts(IScheduleDictionary scheduleDictionary)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return _scheduleDictionaryConflictCollector.GetConflicts(scheduleDictionary,_messageQueueUpdater);
			}
		}

		private void transactions(IEnumerable<IPersonAbsenceAccount> refreshPersonAbsenceAccounts,
									ICollection<IPersonAbsenceAccount> personAbsenceAccounts, 
									IScheduleDictionary scheduleDictionary, 
									IEnumerable<IPersonRequest> personRequests)
		{
			PersistScheduleDictionary(scheduleDictionary);
			PersistPersonAbsenceAccount(refreshPersonAbsenceAccounts, personAbsenceAccounts);
			PersistRequests(personRequests);
		}

		private void PersistRequests(IEnumerable<IPersonRequest> personRequests)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_personRequestPersister.MarkForPersist(_personRequestRepository, personRequests);
				unitOfWork.PersistAll(_messageBrokerModule);
			}
		}

		private void PersistPersonAbsenceAccount(IEnumerable<IPersonAbsenceAccount> refreshPersonAbsenceAccounts,
		                                         ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			try
			{
				using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					_messageQueueUpdater.ReassociateDataWithAllPeople();
					RefreshConflictedPersonAbsenceAccounts(unitOfWork, refreshPersonAbsenceAccounts);
					_personAbsenceAccountRepository.AddRange(personAbsenceAccounts);
					unitOfWork.PersistAll(_messageBrokerModule);
					personAbsenceAccounts.Clear();
				}
			} 
			catch (OptimisticLockException exception)
			{
				throw new OptimisticLockExceptionOnPersonAccount(exception);
			}
		}

		private void RefreshConflictedPersonAbsenceAccounts(IUnitOfWork unitOfWork,
		                                                    IEnumerable<IPersonAbsenceAccount> refreshPersonAbsenceAccounts)
		{
			if (refreshPersonAbsenceAccounts == null)
			{
				return;
			}
			refreshPersonAbsenceAccounts.ForEach(paa =>
			                                     {
			                                     	unitOfWork.Refresh(paa);
			                                     	_personAbsenceAccountRefresher.Refresh(unitOfWork, paa);
			                                     	_personAbsenceAccountValidator.Validate(paa);
			                                     });
		}

		private void PersistScheduleDictionary(IScheduleDictionary scheduleDictionary)
		{
			try
			{
				_scheduleDictionaryBatchPersister.Persist(scheduleDictionary);
			} 
			catch (OptimisticLockException exception)
			{
				throw new OptimisticLockExceptionOnScheduleDictionary(exception);
			}
			catch(ForeignKeyException exception)
			{
				//special case. if fk-exception -> treat it like opt lock
				//because the reason is probably a deleted ref row i db
				var fakeOptLock = new OptimisticLockException(exception.Message, exception);
				throw new OptimisticLockExceptionOnScheduleDictionary(fakeOptLock);
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"),
		 SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"),
		 SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
		private class OptimisticLockExceptionOnPersonAccount : Exception
		{
			public OptimisticLockExceptionOnPersonAccount(OptimisticLockException innerException) : base(null, innerException) {}
		}

		[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"),
		 SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"),
		 SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
		private class OptimisticLockExceptionOnScheduleDictionary : Exception
		{
			public OptimisticLockExceptionOnScheduleDictionary(OptimisticLockException innerException)
				: base(null, innerException) {}
		}
	}
}