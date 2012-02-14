using System;
using System.Collections.ObjectModel;
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
    /// Provides handler for Person Rotation when insert, delete, update incase of message broker call- back.
    /// </summary>
    /// <remarks>
    public class PersonRotationMessageBrokerHandler : IMessageBrokerHandler
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
        private static void HandleInsertRotation(ILoadAggregateById<IRotation> repository, Guid id)
        {
            IRotation rotation = repository.LoadAggregate(id);

            if (rotation != null && !PeopleWorksheet.StateHolder.AllRotations.Contains(rotation))
            {
                PeopleWorksheet.StateHolder.AllRotations.Add(rotation);
            }
        }


        /// <summary>
        /// Deletes the availability.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static bool HandleDeleteRotation(Guid id)
        {
            bool isValid = false;
            IRotation oldRotation = PeopleWorksheet.StateHolder.AllRotations.Where(s => s.Id == id).FirstOrDefault();

            if (oldRotation != null)
            {
                PeopleWorksheet.StateHolder.AllRotations.Remove(oldRotation);
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Updates the contract from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        private void HandleUpdateRotation(ILoadAggregateById<IRotation> repository, Guid id)
        {
            IRotation rotation = repository.LoadAggregate(id);

            if (rotation != null)
            {
                if (HandleDeleteRotation(id))
                {
                    PeopleWorksheet.StateHolder.AllRotations.Add(rotation);

                    UpdateParentRotation(id, rotation);

                    UpdateChildRotation(id, rotation);
                }
            }
        }

        /// <summary>
        /// Updates the child rotation.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rotation">The rotation.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/21/2009
        /// </remarks>
        private void UpdateChildRotation(Guid id, IRotation rotation)
        {
            // Updating child adapters

            var adaptersWithChildren = _stateHolder.
                PersonRotationParentAdapterCollection.Where(s => s.GridControl != null);


            foreach (PersonRotationModelParent adapter in adaptersWithChildren)
            {
                ReadOnlyCollection<PersonRotationModelChild> personRotationChildCollection =
                adapter.GridControl.Tag as ReadOnlyCollection<PersonRotationModelChild>;

                if (personRotationChildCollection != null)
                {

                    var childGridAdaptersWithContract =
                personRotationChildCollection.Where(s => s.CurrentRotation != null && s.CurrentRotation.Id == id);


                    foreach (PersonRotationModelChild childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.CurrentRotation = rotation;
                    }

                }
            }

        }

        /// <summary>
        /// Updates the parent rotation.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rotation">The rotation.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/21/2009
        /// </remarks>
        private void UpdateParentRotation(Guid id, IRotation rotation)
        {

            var adapters =
                _stateHolder.PersonRotationParentAdapterCollection.Where(s => s.CurrentRotation != null && s.CurrentRotation.Id == id);

            foreach (PersonRotationModelParent adapter in adapters)
            {
                adapter.CurrentRotation = rotation;
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
                HandleInsertRotation(new RotationRepository(uow),
                                     _eventMessageArgs.Message.DomainObjectId);
            }
        }

        /// <summary>
        /// Handles the delete when message broker call back.
        /// </summary>
        public void HandleDeleteFromMessageBroker()
        {
            HandleDeleteRotation(_eventMessageArgs.Message.DomainObjectId);
        }

        /// <summary>
        /// Handles the update when message broker call back.
        /// </summary>
        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                HandleUpdateRotation(new RotationRepository(uow),
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
        public PersonRotationMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder stateHolder)
        {
            _eventMessageArgs = e;
            _stateHolder = stateHolder;
        }

        #endregion

        #endregion
    }
}