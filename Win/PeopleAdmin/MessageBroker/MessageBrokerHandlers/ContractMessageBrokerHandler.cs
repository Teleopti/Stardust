using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using System.Linq;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers
{
    /// <summary>
    ///Provides handler for Contract when insert, delete, update incase of message broker call- back.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-12-17
    /// </remarks>
    public class ContractMessageBrokerHandler : IMessageBrokerHandler
    {
        #region Fields - Instance Member

        private EventMessageArgs _eventMessageArgs;
        private FilteredPeopleHolder _stateHolder;

        #endregion

        #region Methods - Instance Member

        #region Private Methods

        /// <summary>
        /// Inserts the contract from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        private static void InsertContractFromMessageBroker(IRepository<IContract> repository, Guid id)
        {
            IContract contract = repository.Get(id);

            if (contract != null && !PeopleWorksheet.StateHolder.ContractCollection.Contains(contract))
            {
                PeopleWorksheet.StateHolder.ContractCollection.Add(contract);
            }
        }

        /// <summary>
        /// Deletes the contract from message broker.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-18
        /// </remarks>
        private static void DeleteContractFromMessageBroker(Guid id)
        {
            DeleteContract(id);
        }

        /// <summary>
        /// Deletes the contract.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        private static bool DeleteContract(Guid id)
        {
            bool isValid = false;
            IContract oldContract = PeopleWorksheet.StateHolder.ContractCollection.Where(s => s.Id == id).
                FirstOrDefault();

            

            if (oldContract != null)
            {
                PeopleWorksheet.StateHolder.ContractCollection.Remove(oldContract);
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Updates the contract from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        private void UpdateContractFromMessageBroker(IRepository<IContract> repository, Guid id)
        {
            IContract contract = repository.Get(id);

            if (contract != null)
            {
                if (DeleteContract(id))
                {
                    PeopleWorksheet.StateHolder.ContractCollection.Add(contract);

                    UpdateParentContract(id, contract);

                    UpdateChildContract(id, contract);
                }
            }
        }

        /// <summary>
        /// Updates the child contract.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="contract">The contract.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        private void UpdateChildContract(Guid id, IContract contract)
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
                personPeriodChildCollection.Where(s => s.Contract.Id == id).ToList();


                    foreach (PersonPeriodChildModel childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.Contract = contract;
                    }

                }
            }
        }

        /// <summary>
        /// Updates the parent contract.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="contract">The contract.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        private void UpdateParentContract(Guid id, IContract contract)
        {
            IList<PersonPeriodModel> adapters =
                _stateHolder.PersonPeriodGridViewCollection.Where(s => s.Contract != null && s.Contract.Id == id).ToList();

            foreach (PersonPeriodModel adapter in adapters)
            {
                adapter.Contract = contract;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles the insert when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public void HandleInsertFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InsertContractFromMessageBroker(new ContractRepository(uow),
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
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public void HandleDeleteFromMessageBroker()
        {
            DeleteContractFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
        }

        /// <summary>
        /// Handles the update when message broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                UpdateContractFromMessageBroker(new ContractRepository(uow),
                                                _eventMessageArgs.Message.DomainObjectId);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractMessageBrokerHandler"/> class.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        public ContractMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder stateHolder)
        {
            _eventMessageArgs = e;
            _stateHolder = stateHolder;
        }

        #endregion

        #endregion
    }
}