using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Provides handler for Rule Set Bag Schedule when insert, delete, update incase of message broker call- back.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 1/12/2009
    /// </remarks>
    public class RuleSetBagMessageBrokerHandler : IMessageBrokerHandler
    {

        private EventMessageArgs _eventMessageArgs;
        private FilteredPeopleHolder _filteredPeopleHolder;

        /// <summary>
        /// Inserts the part time percentage from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-19
        /// </remarks>
        private void InsertRuleSetBagFromMessageBroker(RuleSetBagRepository repository, Guid id)
        {
            IRuleSetBag ruleSetBag = repository.Get(id);

            if (ruleSetBag != null &&
                !_filteredPeopleHolder.RuleSetBagCollection.Contains(ruleSetBag))
            {
                _filteredPeopleHolder.RuleSetBagCollection.Add(ruleSetBag);
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
        private void DeleteRuleSetBagFromMessageBroker(Guid id)
        {
            DeleteRuleSetBag(id);
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
        private bool DeleteRuleSetBag(Guid id)
        {
            bool isValid = false;
            IRuleSetBag oldRuleSetBag = _filteredPeopleHolder.RuleSetBagCollection.Where(s => s.Id == id).
                FirstOrDefault();

            if (oldRuleSetBag != null)
            {
                _filteredPeopleHolder.RuleSetBagCollection.Remove(oldRuleSetBag);
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
        private void UpdateRuleSetBagFromMessageBroker(RuleSetBagRepository repository, Guid id)
        {
            IRuleSetBag ruleSetBag = repository.Get(id);

            if (ruleSetBag != null)
            {
                if (DeleteRuleSetBag(id))
                {
                    _filteredPeopleHolder.RuleSetBagCollection.Add(ruleSetBag);

                    UpdatParenteRuleSet(id, ruleSetBag);
                    
                    UpdateChildRuleSet(id, ruleSetBag);

                }
            }
        }

        /// <summary>
        /// Updats the parente rule set.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="ruleSetBag">The rule set bag.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        private void UpdatParenteRuleSet(Guid id, IRuleSetBag ruleSetBag)
        {
            IList<PersonPeriodModel> adapters =
               _filteredPeopleHolder.PersonPeriodGridViewCollection.Where(s => s.RuleSetBag!= null && s.RuleSetBag.Id == id).ToList();

            foreach (PersonPeriodModel adapter in adapters)
            {
                adapter.RuleSetBag = ruleSetBag;
            }
            
        }

        /// <summary>
        /// Updates the child rule set.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="ruleSetBag">The rule set bag.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/14/2009
        /// </remarks>
        private void UpdateChildRuleSet(Guid id, IRuleSetBag ruleSetBag)
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
                    personPeriodChildCollection.Where(s => s.RuleSetBag != null && s.RuleSetBag.Id == id).ToList();


                    foreach (PersonPeriodChildModel childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.RuleSetBag = ruleSetBag;
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
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-18
        /// </remarks>
        public void HandleInsertFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InsertRuleSetBagFromMessageBroker(new RuleSetBagRepository(uow),
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
        /// Created date: 2008-12-18
        /// </remarks>
        public void HandleDeleteFromMessageBroker()
        {
            DeleteRuleSetBagFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
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
        /// Created date: 2008-12-18
        /// </remarks>
        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                UpdateRuleSetBagFromMessageBroker(new RuleSetBagRepository(uow),
                                                  _eventMessageArgs.Message.DomainObjectId);
            }
        }

        public RuleSetBagMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder filteredPeopleHolder)
        {
            _eventMessageArgs = e;
            _filteredPeopleHolder = filteredPeopleHolder;
        }
    }
}