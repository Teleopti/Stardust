using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Win.PeopleAdmin.MessageBroker.MessageBrokerHandlers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 1/12/2009
    /// </remarks>
    public class OptionalColumnMessageBrokerHandler : IMessageBrokerHandler
    {
        #region Fields - Instance Member

        private EventMessageArgs _eventMessageArgs;
        private FilteredPeopleHolder _filteredPeopleHolder;

        #endregion

        #region Methods - Instance Member

        #region Private Methods

        /// <summary>
        /// Inserts the optional column from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/12/2009
        /// </remarks>
        private void InsertOptionalColumnFromMessageBroker(IRepository<IOptionalColumn> repository, Guid id)
        {
            IOptionalColumn optionalColumn = repository.Get(id);

            if (optionalColumn != null && !_filteredPeopleHolder.OptionalColumnCollection.Contains(optionalColumn))
            {
                _filteredPeopleHolder.OptionalColumnCollection.Add(optionalColumn);
            }
        }

        /// <summary>
        /// Deletes the optional column from message broker.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/12/2009
        /// </remarks>
        private void DeleteOptionalColumnFromMessageBroker(Guid id)
        {
           DeleteOptionalColumn(id);
        }

        /// <summary>
        /// Deletes the optional column.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/12/2009
        /// </remarks>
        private bool DeleteOptionalColumn(Guid id)
        {
            bool isValid = false;
            IOptionalColumn oldOptionalColumn = _filteredPeopleHolder.OptionalColumnCollection.Where(s => s.Id == id).
                FirstOrDefault();

            if (oldOptionalColumn != null)
            {
                _filteredPeopleHolder.OptionalColumnCollection.Remove(oldOptionalColumn);
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Updates the optional column from message broker.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/12/2009
        /// </remarks>
        private void UpdateOptionalColumnFromMessageBroker(IRepository<IOptionalColumn> repository, Guid id)
        {
            IOptionalColumn optionalColumn = repository.Get(id);

            if (optionalColumn != null)
            {
                if (DeleteOptionalColumn(id))
                {
                    InsertOptionalColumnFromMessageBroker(repository, id);
                }
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
        /// Created date: 1/12/2009
        /// </remarks>
        public void HandleInsertFromMessageBroker()
        {
            InsertOptionalColumnFromMessageBroker(new OptionalColumnRepository(_filteredPeopleHolder.UnitOfWork),
                                         _eventMessageArgs.Message.DomainObjectId);
        }

      

        /// <summary>
        /// Handles the delete when mesmsage broker call back.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-17
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/12/2009
        /// </remarks>
        public void HandleDeleteFromMessageBroker()
        {
            DeleteOptionalColumnFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
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
        /// Created date: 1/12/2009
        /// </remarks>
        public void HandleUpdateFromMessageBroker()
        {
            UpdateOptionalColumnFromMessageBroker(new OptionalColumnRepository(_filteredPeopleHolder.UnitOfWork),
                                        _eventMessageArgs.Message.DomainObjectId);
        }
       

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMessageBrokerHandler"/> class.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-12-18
        /// </remarks>
        public OptionalColumnMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder filteredPeopleHolder)
        {
            _eventMessageArgs = e;
            _filteredPeopleHolder = filteredPeopleHolder;
        }

        #endregion

        #endregion
    }
}
