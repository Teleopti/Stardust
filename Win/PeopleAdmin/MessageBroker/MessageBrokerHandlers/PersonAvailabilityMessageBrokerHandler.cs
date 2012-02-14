using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using System.Linq;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers
{
    /// <summary>
    /// Provides handler for Person Availability when insert, delete, update incase of message broker call- back.
    /// </summary>
    /// <remarks>
    public class PersonAvailabilityMessageBrokerHandler : IMessageBrokerHandler
    {
        #region Fields - Instance Member

        private readonly EventMessageArgs _eventMessageArgs;
        private readonly FilteredPeopleHolder _stateHolder;

        #endregion

        #region Methods - Instance Member

        #region Private Methods

        /// <summary>
        /// Inserts the contract from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        private static void HandleInsertAvailability(ILoadAggregateById<IAvailabilityRotation> repository, Guid id)
        {
            IAvailabilityRotation availability = repository.LoadAggregate(id);

            if (availability != null && !PeopleWorksheet.StateHolder.AllAvailabilities.Contains(availability))
            {
                PeopleWorksheet.StateHolder.AllAvailabilities.Add(availability);
            }
        }


        /// <summary>
        /// Deletes the availability.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static bool HandleDeleteAvailability(Guid id)
        {
            bool isValid = false;
            IAvailabilityRotation oldAvaialability = PeopleWorksheet.StateHolder.AllAvailabilities.Where(s => s.Id == id).FirstOrDefault();

            if (oldAvaialability != null)
            {
                PeopleWorksheet.StateHolder.AllAvailabilities.Remove(oldAvaialability);
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Updates the contract from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        private void HandleUpdateAvailability(ILoadAggregateById<IAvailabilityRotation> repository, Guid id)
        {
            IAvailabilityRotation availability = repository.LoadAggregate(id);

            if (availability != null)
            {
                if (HandleDeleteAvailability(id))
                {
                    PeopleWorksheet.StateHolder.AllAvailabilities.Add(availability);

                    UpdateParentAvailability(id, availability);

                    UpdateChildAvailability(id, availability);
                }
            }
        }

        /// <summary>
        /// Updates the child availability.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="availability">The availability.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/21/2009
        /// </remarks>
        private void UpdateChildAvailability(Guid id, IAvailabilityRotation availability)
        {
            // Updating child adapters

            var adaptersWithChildren = _stateHolder.
                PersonAvailabilityParentAdapterCollection.Where(s => s.GridControl != null);


            foreach (PersonAvailabilityModelParent adapter in adaptersWithChildren)
            {
                ReadOnlyCollection<PersonAvailabilityModelChild> personAvailabilityChildCollection =
                adapter.GridControl.Tag as ReadOnlyCollection<PersonAvailabilityModelChild>;

                if (personAvailabilityChildCollection != null)
                {

                    var childGridAdaptersWithContract =
                personAvailabilityChildCollection.Where(s => s.CurrentRotation != null && s.CurrentRotation.Id == id);


                    foreach (PersonAvailabilityModelChild childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.CurrentRotation = availability;
                    }

                }
            }
        }

        private void UpdateParentAvailability(Guid id, IAvailabilityRotation availability)
        {

            var adapters =
                _stateHolder.PersonAvailabilityParentAdapterCollection.Where(s => s.CurrentRotation != null 
                    && s.CurrentRotation.Id == id);

            foreach (PersonAvailabilityModelParent adapter in adapters)
            {
                adapter.CurrentRotation = availability;
            }

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles the insert when message broker call back.
        /// </summary>
        public void HandleInsertFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                HandleInsertAvailability(new AvailabilityRepository(uow),
                                         _eventMessageArgs.Message.DomainObjectId);
            }
        }

        /// <summary>
        /// Handles the delete when message broker call back.
        /// </summary>
        public void HandleDeleteFromMessageBroker()
        {
            HandleDeleteAvailability(_eventMessageArgs.Message.DomainObjectId);
        }

        /// <summary>
        /// Handles the update when message broker call back.
        /// </summary>
        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                HandleUpdateAvailability(new AvailabilityRepository(uow),
                                         _eventMessageArgs.Message.DomainObjectId);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractMessageBrokerHandler"/> class.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="stateHolder">The state holder.</param>
        public PersonAvailabilityMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder stateHolder)
        {
            _eventMessageArgs = e;
            _stateHolder = stateHolder;
        }

        #endregion

        #endregion
    }
}