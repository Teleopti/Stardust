using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers
{
    /// <summary>
    /// Provides handler for Contract Schedule when insert, delete, update incase of message broker call- back.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-12-19
    /// </remarks>
    public class ContractScheduleMessageBrokerHandler : IMessageBrokerHandler
    {
        private EventMessageArgs _eventMessageArgs;
        private FilteredPeopleHolder _stateHolder;

        /// <summary>
        /// Inserts the contract schedule from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-19
        /// </remarks>
        private static void InsertContractScheduleFromMessageBroker(IRepository<IContractSchedule> repository, Guid id)
        {
            IContractSchedule contractSchedule = repository.Get(id);

            if (contractSchedule != null &&
                !PeopleWorksheet.StateHolder.ContractScheduleCollection.Contains(contractSchedule))
            {
                initializeConractSchedule(contractSchedule);
                PeopleWorksheet.StateHolder.ContractScheduleCollection.Add(contractSchedule);
            }
        }

        private static void initializeConractSchedule(IContractSchedule contractSchedule)
        {
            if (!LazyLoadingManager.IsInitialized(contractSchedule))
            {
                LazyLoadingManager.Initialize(contractSchedule);
            }
            if (!LazyLoadingManager.IsInitialized(contractSchedule.ContractScheduleWeeks))
            {
                LazyLoadingManager.Initialize(contractSchedule.ContractScheduleWeeks);
                foreach (IContractScheduleWeek contractScheduleWeek in contractSchedule.ContractScheduleWeeks)
                {
                    Array dayOfWeekArray = Enum.GetValues(typeof(DayOfWeek));
                    foreach (DayOfWeek dayOfWeek in dayOfWeekArray)
                    {
                        contractScheduleWeek.IsWorkday(dayOfWeek);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the contract schedule from message broker.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-19
        /// </remarks>
        private static void DeleteContractScheduleFromMessageBroker(Guid id)
        {
            DeleteContractSchedule(id);
        }

        /// <summary>
        /// Deletes the contract schedule.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-19
        /// </remarks>
        private static bool DeleteContractSchedule(Guid id)
        {
            bool isValid = false;
            IContractSchedule oldContractSchedule = PeopleWorksheet.StateHolder.ContractScheduleCollection.Where(s => s.Id == id).
                FirstOrDefault();

            if (oldContractSchedule != null)
            {
                PeopleWorksheet.StateHolder.ContractScheduleCollection.Remove(oldContractSchedule);
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Updates the contract schedule from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-19
        /// </remarks>
        private void UpdateContractScheduleFromMessageBroker(IRepository<IContractSchedule> repository, Guid id)
        {
            IContractSchedule contractSchedule = repository.Get(id);

            if (contractSchedule != null)
            {
                if (DeleteContractSchedule(id))
                {
                    initializeConractSchedule(contractSchedule);
                    PeopleWorksheet.StateHolder.ContractScheduleCollection.Add(contractSchedule);

                    UpdateParentContractSchedule(id, contractSchedule);

                    UpdateChildContractSchedule(id, contractSchedule);
                }
            }
        }

        /// <summary>
        /// Updates the parent contract schedule.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="contractSchedule">The contract schedule.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        private void UpdateParentContractSchedule(Guid id, IContractSchedule contractSchedule)
        {
            IList<PersonPeriodModel> adapters =
                _stateHolder.PersonPeriodGridViewCollection.Where(s =>  s.ContractSchedule != null && 
                    s.ContractSchedule.Id == id).ToList();

            foreach (PersonPeriodModel adapter in adapters)
            {
                adapter.ContractSchedule = contractSchedule;
            }
        }

        /// <summary>
        /// Updates the child contract schedule.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="contractSchedule">The contract schedule.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        private void UpdateChildContractSchedule(Guid id, IContractSchedule contractSchedule)
        {
            // Updating child adapters

            IList<PersonPeriodModel> adaptersWithChildren = _stateHolder.
                PersonPeriodGridViewCollection.Where(s => s.GridControl != null).ToList();

            foreach (PersonPeriodModel adapter in adaptersWithChildren)
            {
                ReadOnlyCollection<PersonPeriodChildModel> personPeriodChildCollection =
                adapter.GridControl.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

                if (personPeriodChildCollection != null)
                {
                    IList<PersonPeriodChildModel> childGridAdaptersWithContract =
                    personPeriodChildCollection.Where(s => s.ContractSchedule.Id == id).ToList();


                    foreach (PersonPeriodChildModel childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.ContractSchedule = contractSchedule;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the insert when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public void HandleInsertFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InsertContractScheduleFromMessageBroker(new ContractScheduleRepository(uow),
                                                        _eventMessageArgs.Message.DomainObjectId);
            }
        }

        /// <summary>
        /// Handles the delete when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public void HandleDeleteFromMessageBroker()
        {
            DeleteContractScheduleFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
        }

        /// <summary>
        /// Handles the update when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                UpdateContractScheduleFromMessageBroker(new ContractScheduleRepository(uow),
                                                        _eventMessageArgs.Message.DomainObjectId);
            }
        }

        public ContractScheduleMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder stateHolder)
        {
            _eventMessageArgs = e;
            _stateHolder = stateHolder;
        }
    }
}