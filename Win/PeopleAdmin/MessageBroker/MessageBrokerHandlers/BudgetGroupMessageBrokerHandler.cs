using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers
{
    public class BudgetGroupMessageBrokerHandler : IMessageBrokerHandler
    {

        private EventMessageArgs _eventMessageArgs;
        private FilteredPeopleHolder _filteredPeopleHolder;

        private void InsertBudgetGroupFromMessageBroker(IBudgetGroupRepository repository, Guid id)
        {
            IBudgetGroup budgetGroup = repository.Get(id);

            if (budgetGroup != null &&
                !_filteredPeopleHolder.BudgetGroupCollection.Contains(budgetGroup))
            {
                _filteredPeopleHolder.BudgetGroupCollection.Add(budgetGroup);
            }
        }

        private void DeleteBudgetGroupFromMessageBroker(Guid id)
        {
            DeleteBudgetGroup(id);
        }

        private bool DeleteBudgetGroup(Guid id)
        {
            bool isValid = false;
            IBudgetGroup oldBudgetGroup = _filteredPeopleHolder.BudgetGroupCollection.Where(s => s.Id == id).
                FirstOrDefault();

            if (oldBudgetGroup != null)
            {
                _filteredPeopleHolder.BudgetGroupCollection.Remove(oldBudgetGroup);
                isValid = true;
            }

            return isValid;
        }

        private void UpdateBudgetGroupFromMessageBroker(BudgetGroupRepository repository, Guid id)
        {
            IBudgetGroup budgetGroup = repository.Get(id);

            if (budgetGroup != null)
            {
                if (DeleteBudgetGroup(id))
                {
                    _filteredPeopleHolder.BudgetGroupCollection.Add(budgetGroup);

                    UpdatParentBudgetGroup(id, budgetGroup);
                    
                    UpdateChildBudgetGroup(id, budgetGroup);

                }
            }
        }

        private void UpdatParentBudgetGroup(Guid id, IBudgetGroup budgetGroup)
        {
            IList<PersonPeriodModel> adapters =
               _filteredPeopleHolder.PersonPeriodGridViewCollection.Where(s => s.BudgetGroup!= null && s.BudgetGroup.Id == id).ToList();

            foreach (PersonPeriodModel adapter in adapters)
            {
                adapter.BudgetGroup = budgetGroup;
            }
            
        }

        private void UpdateChildBudgetGroup(Guid id, IBudgetGroup budgetGroup)
        {
            // Updating child adapters

            IList<PersonPeriodModel> adaptersWithChildren = _filteredPeopleHolder.
                PersonPeriodGridViewCollection.Where(s => s.GridControl != null).ToList();

            foreach (PersonPeriodModel adapter in adaptersWithChildren)
            {
                ReadOnlyCollection<PersonPeriodChildModel> personPeriodChildCollection =
                adapter.GridControl.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

                if (personPeriodChildCollection != null)
                {
                    IList<PersonPeriodChildModel> childGridAdaptersWithContract =
                    personPeriodChildCollection.Where(s => s.BudgetGroup != null && s.BudgetGroup.Id == id).ToList();


                    foreach (PersonPeriodChildModel childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.BudgetGroup = budgetGroup;
                    }
                }
            }
        }

        public void HandleInsertFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InsertBudgetGroupFromMessageBroker(new BudgetGroupRepository(uow),
                                                  _eventMessageArgs.Message.DomainObjectId);
            }
        }

        public void HandleDeleteFromMessageBroker()
        {
            DeleteBudgetGroupFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
        }

        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                UpdateBudgetGroupFromMessageBroker(new BudgetGroupRepository(uow),
                                                  _eventMessageArgs.Message.DomainObjectId);
            }
        }

        public BudgetGroupMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder filteredPeopleHolder)
        {
            _eventMessageArgs = e;
            _filteredPeopleHolder = filteredPeopleHolder;
        }
    }
}