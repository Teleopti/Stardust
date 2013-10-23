using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleScreenRetryingPersister : IScheduleScreenPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IWriteProtectionRepository _writeProtectionRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPersonRequestPersister _personRequestPersister;
		private readonly IPersonAbsenceAccountConflictCollector _personAbsenceAccountConflictCollector;
        private readonly ITraceableRefreshService _traceableRefreshService;
		private readonly IScheduleDictionaryConflictCollector _scheduleDictionaryConflictCollector;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;
		private readonly IScheduleDictionaryBatchPersister _scheduleDictionaryBatchPersister;
		private readonly IOwnMessageQueue _messageQueueUpdater;

        public ScheduleScreenRetryingPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,  IWriteProtectionRepository writeProtectionRepository,
		                                       IPersonRequestRepository personRequestRepository,
		                                       IPersonAbsenceAccountRepository personAbsenceAccountRepository,
		                                       IPersonRequestPersister personRequestPersister,
		                                       IPersonAbsenceAccountConflictCollector personAbsenceAccountConflictCollector,
		                                       ITraceableRefreshService traceableRefreshService,
		                                       IScheduleDictionaryConflictCollector scheduleDictionaryConflictCollector,
		                                       IMessageBrokerIdentifier messageBrokerIdentifier,
												IScheduleDictionaryBatchPersister scheduleDictionaryBatchPersister,
												IOwnMessageQueue messageQueueUpdater)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
	        _writeProtectionRepository = writeProtectionRepository;
			_personRequestRepository = personRequestRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_personRequestPersister = personRequestPersister;
			_personAbsenceAccountConflictCollector = personAbsenceAccountConflictCollector;
			_traceableRefreshService = traceableRefreshService;
			_scheduleDictionaryConflictCollector = scheduleDictionaryConflictCollector;
			_messageBrokerIdentifier = messageBrokerIdentifier;
        	_scheduleDictionaryBatchPersister = scheduleDictionaryBatchPersister;
        	_messageQueueUpdater = messageQueueUpdater;
		}

		public IScheduleScreenPersisterResult TryPersist(IScheduleDictionary scheduleDictionary, 
															ICollection<IPersonWriteProtectionInfo> persons,
															IEnumerable<IPersonRequest> personRequests,
															ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			var unitOfWorkFactory = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory();
			if(!persons.IsEmpty())
				saveWriteProtectionInSeparateTransaction(unitOfWorkFactory, persons);
			bool retry;
			IEnumerable<IPersonAbsenceAccount> conflictedPersonAbsenceAccounts = null;
			do
			{
				try
				{
					transactions(unitOfWorkFactory, conflictedPersonAbsenceAccounts, personAbsenceAccounts, scheduleDictionary, personRequests);
					retry = false;
				}
				catch (OptimisticLockExceptionOnPersonAccount)
				{
					conflictedPersonAbsenceAccounts = GetPersonAbsenceAccountConflicts(unitOfWorkFactory, personAbsenceAccounts);
					retry = true;
				}
				catch (OptimisticLockExceptionOnScheduleDictionary)
				{
					return scheduleDictionaryConflictResult(scheduleDictionary, unitOfWorkFactory);
				}
				catch (ForeignKeyExceptionOnScheduleDictionary)
				{
					return scheduleDictionaryConflictResult(scheduleDictionary, unitOfWorkFactory);
				}
				catch (PersonAssignmentConstraintViolationOnScheduleDictionary)
				{
					return scheduleDictionaryConflictResult(scheduleDictionary, unitOfWorkFactory);
				}
				catch (OptimisticLockException)
				{
					return new ScheduleScreenPersisterResult { Saved = false };
				}
			} while (retry);
			return new ScheduleScreenPersisterResult {Saved = true};
		}

		private IScheduleScreenPersisterResult scheduleDictionaryConflictResult(IScheduleDictionary scheduleDictionary, IUnitOfWorkFactory unitOfWorkFactory)
		{
			var conflicts = GetScheduleDictionaryConflicts(unitOfWorkFactory, scheduleDictionary);
			return new ScheduleScreenPersisterResult {Saved = false, ScheduleDictionaryConflicts = conflicts};
		}

		private void saveWriteProtectionInSeparateTransaction(IUnitOfWorkFactory unitOfWorkFactory, ICollection<IPersonWriteProtectionInfo> personWriteProtectionInfos)
		{
			using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_writeProtectionRepository.AddRange(personWriteProtectionInfos);
				unitOfWork.PersistAll(_messageBrokerIdentifier);
				personWriteProtectionInfos.Clear();
			}
		}

		private IEnumerable<IPersonAbsenceAccount> GetPersonAbsenceAccountConflicts(IUnitOfWorkFactory unitOfWorkFactory, IEnumerable<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return _personAbsenceAccountConflictCollector.GetConflicts(uow, personAbsenceAccounts);
			}
		}

		private IEnumerable<PersistConflict> GetScheduleDictionaryConflicts(IUnitOfWorkFactory unitOfWorkFactory, IScheduleDictionary scheduleDictionary)
		{
			using (unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return _scheduleDictionaryConflictCollector.GetConflicts(scheduleDictionary,_messageQueueUpdater);
			}
		}

		private void transactions(IUnitOfWorkFactory unitOfWorkFactory, 
									IEnumerable<IPersonAbsenceAccount> refreshPersonAbsenceAccounts,
									ICollection<IPersonAbsenceAccount> personAbsenceAccounts, 
									IScheduleDictionary scheduleDictionary, 
									IEnumerable<IPersonRequest> personRequests)
		{
			PersistScheduleDictionary(scheduleDictionary);
			PersistPersonAbsenceAccount(unitOfWorkFactory, refreshPersonAbsenceAccounts, personAbsenceAccounts);
			PersistRequests(unitOfWorkFactory, personRequests);
		}

		private void PersistRequests(IUnitOfWorkFactory unitOfWorkFactory, IEnumerable<IPersonRequest> personRequests)
		{
			using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_personRequestPersister.MarkForPersist(_personRequestRepository, personRequests);
				unitOfWork.PersistAll(_messageBrokerIdentifier);
			}
		}

		private void PersistPersonAbsenceAccount(IUnitOfWorkFactory unitOfWorkFactory, 
												 IEnumerable<IPersonAbsenceAccount> refreshPersonAbsenceAccounts,
		                                         ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			try
			{
				using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					_messageQueueUpdater.ReassociateDataWithAllPeople();
					RefreshConflictedPersonAbsenceAccounts(unitOfWork, refreshPersonAbsenceAccounts);
					_personAbsenceAccountRepository.AddRange(personAbsenceAccounts);
					unitOfWork.PersistAll(_messageBrokerIdentifier);
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
				var foundAccount = _personAbsenceAccountRepository.Get(paa.Id.GetValueOrDefault());
				if (foundAccount != null)
				{
					var removedAccounts = paa.AccountCollection().Except(foundAccount.AccountCollection()).ToList();
					foreach (IAccount removedAccount in removedAccounts)
					{
						paa.Remove(removedAccount);
					}
					unitOfWork.Remove(foundAccount);
					unitOfWork.Refresh(paa);
				}
				paa.AccountCollection().ForEach(_traceableRefreshService.Refresh);
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
			catch (ForeignKeyException exception)
			{
				//special case.
				//because the reason is probably a deleted ref row i db
				throw new ForeignKeyExceptionOnScheduleDictionary(exception);
			}
			catch (ConstraintViolationException exception)
			{
				if (exception.InnerException.Message.Contains("'UQ_PersonAssignment_Date_Scenario_Person'"))
				{
					// if 2 persons try to save a PA for the same person and date, handle it like a conflict
					throw new PersonAssignmentConstraintViolationOnScheduleDictionary(exception);
				}
				throw;
			}
		}

		private class OptimisticLockExceptionOnPersonAccount : Exception
		{
			public OptimisticLockExceptionOnPersonAccount(OptimisticLockException innerException) : base(null, innerException) {}
		}

		private class OptimisticLockExceptionOnScheduleDictionary : Exception
		{
			public OptimisticLockExceptionOnScheduleDictionary(OptimisticLockException innerException) : base(null, innerException) {}
		}

		private class ForeignKeyExceptionOnScheduleDictionary : Exception
		{
			public ForeignKeyExceptionOnScheduleDictionary(ForeignKeyException innerException) : base(null, innerException) { }
		}

		private class PersonAssignmentConstraintViolationOnScheduleDictionary : Exception
		{
			public PersonAssignmentConstraintViolationOnScheduleDictionary(ConstraintViolationException innerException) : base(null, innerException) { }
		}
	}
}