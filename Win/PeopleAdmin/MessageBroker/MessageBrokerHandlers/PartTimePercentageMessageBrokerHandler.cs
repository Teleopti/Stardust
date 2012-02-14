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
    public class PartTimePercentageMessageBrokerHandler:IMessageBrokerHandler
    {
        private readonly EventMessageArgs _eventMessageArgs;
        private readonly FilteredPeopleHolder _stateHolder;

        public PartTimePercentageMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder stateHolder)
        {
            _eventMessageArgs = e;
            _stateHolder = stateHolder;
        }

        private static void InsertPartTimePercentageFromMessageBroker(IRepository<IPartTimePercentage> repository, Guid id)
        {
            IPartTimePercentage partTimePercentage = repository.Get(id);

            if (partTimePercentage != null &&
                !PeopleWorksheet.StateHolder.PartTimePercentageCollection.Contains(partTimePercentage))
            {
                PeopleWorksheet.StateHolder.PartTimePercentageCollection.Add(partTimePercentage);
            }
        }

        private static void DeletePartTimePercentageFromMessageBroker(Guid id)
        {
            DeletePartTimePercentage(id);
        }

        private static bool DeletePartTimePercentage(Guid id)
        {
            bool isValid = false;
            IPartTimePercentage oldPartTimePercentage = PeopleWorksheet.StateHolder.PartTimePercentageCollection.Where(s => s.Id == id).
                FirstOrDefault();

            if (oldPartTimePercentage != null)
            {
                PeopleWorksheet.StateHolder.PartTimePercentageCollection.Remove(oldPartTimePercentage);
                isValid = true;
            }

            return isValid;
        }

        private void UpdatePartTimePercentageFromMessageBroker(IRepository<IPartTimePercentage> repository, Guid id)
        {
            IPartTimePercentage partTimePercentage = repository.Get(id);

            if (partTimePercentage != null)
            {
                if (DeletePartTimePercentage(id))
                {
                    PeopleWorksheet.StateHolder.PartTimePercentageCollection.Add(partTimePercentage);

                    UpdateParentPartTimePercentage(id, partTimePercentage);

                    UpdateChildPartTimePercentage(id, partTimePercentage);
                }
            }
        }

        private void UpdateParentPartTimePercentage(Guid id, IPartTimePercentage partTimePercentage)
        {
            IList<PersonPeriodModel> adapters =
               _stateHolder.PersonPeriodGridViewCollection.Where(s => s.PartTimePercentage != null && 
                   s.PartTimePercentage.Id == id).ToList();

            foreach (PersonPeriodModel adapter in adapters)
            {
                adapter.PartTimePercentage = partTimePercentage;
            }
        }

        private void UpdateChildPartTimePercentage(Guid id, IPartTimePercentage partTimePercentage)
        {
            IList<PersonPeriodModel> adaptersWithChildren = _stateHolder.
                PersonPeriodGridViewCollection.Where(s => s.GridControl != null).ToList();

            foreach (PersonPeriodModel adapter in adaptersWithChildren)
            {
                ReadOnlyCollection<PersonPeriodChildModel> personPeriodChildCollection =
                adapter.GridControl.Tag as ReadOnlyCollection<PersonPeriodChildModel>;

                if (personPeriodChildCollection != null)
                {
                    IList<PersonPeriodChildModel> childGridAdaptersWithContract =
                    personPeriodChildCollection.Where(s => s.PartTimePercentage.Id == id).ToList();

                    foreach (PersonPeriodChildModel childGridViewAdapter in childGridAdaptersWithContract)
                    {
                        childGridViewAdapter.PartTimePercentage = partTimePercentage;
                    }
                }
            }
        }

        public void HandleInsertFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                InsertPartTimePercentageFromMessageBroker(new PartTimePercentageRepository(uow),
                                                          _eventMessageArgs.Message.DomainObjectId);
            }
        }

        public void HandleDeleteFromMessageBroker()
        {
            DeletePartTimePercentageFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
        }

        public void HandleUpdateFromMessageBroker()
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                UpdatePartTimePercentageFromMessageBroker(new PartTimePercentageRepository(uow),
                                                          _eventMessageArgs.Message.DomainObjectId);
            }
        }
    }
}