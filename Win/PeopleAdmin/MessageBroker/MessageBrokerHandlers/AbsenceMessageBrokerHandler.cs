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
	class AbsenceMessageBrokerHandler : IMessageBrokerHandler
	{
		private EventMessageArgs _eventMessageArgs;
		private FilteredPeopleHolder _filteredPeopleHolder;

		public AbsenceMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder filteredPeopleHolder)
		{
			_eventMessageArgs = e;
			_filteredPeopleHolder = filteredPeopleHolder;
		}

		private void InsertAbsenceFromMessageBroker(AbsenceRepository repository, Guid id)
		{
			IAbsence absence = repository.Get(id);

			if (absence != null && !_filteredPeopleHolder.FilteredAbsenceCollection.Contains(absence))
			{
				_filteredPeopleHolder.AddAbsence(absence);
			}
		}

		private void DeleteAbsenceFromMessageBroker(Guid id)
		{
			DeleteAbsence(id);
		}

		private bool DeleteAbsence(Guid id)
		{
			bool isValid = false;
			IAbsence oldAbsence = _filteredPeopleHolder.FilteredAbsenceCollection.FirstOrDefault(s => s.Id == id);

			if (oldAbsence != null)
			{
				_filteredPeopleHolder.RemoveAbsence(oldAbsence);
				isValid = true;
			}

			return isValid;
		}

		private void UpdateAbsenceFromMessageBroker(Guid id)
		{
			IAbsence absence;

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				absence = (new AbsenceRepository(uow)).Get(id);

			}
			_filteredPeopleHolder.UnitOfWork.Refresh(absence);

			if (absence != null)
			{
				if (DeleteAbsence(id))
				{
					_filteredPeopleHolder.AddAbsence(absence);
				}

				UpdateParentPersonAccount(id, absence);

				UpdateChildPersonAccount(id, absence);
			}
		}

		private void UpdateChildPersonAccount(Guid id, IAbsence absence)
		{
			IList<IPersonAccountModel> adaptersWithChildren = _filteredPeopleHolder.
				PersonAccountGridViewAdaptorCollection.Where(s => s.GridControl != null).ToList();

			foreach (IPersonAccountModel adapter in adaptersWithChildren)
			{
				ReadOnlyCollection<IPersonAccountChildModel> personAccountChildCollection =
				adapter.GridControl.Tag as ReadOnlyCollection<IPersonAccountChildModel>;

				if (personAccountChildCollection != null)
				{

					IList<IPersonAccountChildModel> childGridAdaptersWithAbsence =
				   personAccountChildCollection.Where(s => s.TrackingAbsence != null && s.TrackingAbsence.Id == id).ToList();


					foreach (IPersonAccountChildModel childGridViewAdapter in childGridAdaptersWithAbsence)
					{
						childGridViewAdapter.TrackingAbsence = absence;
					}
				}
			}
		}

		private void UpdateParentPersonAccount(Guid id, IAbsence absence)
		{
			IList<IPersonAccountModel> adapters =
		   _filteredPeopleHolder.PersonAccountGridViewAdaptorCollection.Where(s => s.TrackingAbsence != null && s.TrackingAbsence.Id == id).ToList();

			foreach (IPersonAccountModel adapter in adapters)
			{
				adapter.TrackingAbsence = absence;
			}
		}

		public void HandleInsertFromMessageBroker()
		{
			InsertAbsenceFromMessageBroker(new AbsenceRepository(_filteredPeopleHolder.UnitOfWork),
										 _eventMessageArgs.Message.DomainObjectId);
		}

		public void HandleDeleteFromMessageBroker()
		{
			DeleteAbsenceFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
		}

		public void HandleUpdateFromMessageBroker()
		{
			UpdateAbsenceFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
		}
	}
}
