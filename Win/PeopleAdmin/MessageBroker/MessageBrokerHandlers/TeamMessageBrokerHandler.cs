using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
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
    public class TeamMessageBrokerHandler : IMessageBrokerHandler
    {
    	private EventMessageArgs _eventMessageArgs;
        private FilteredPeopleHolder _filteredPeopleHolder;

		public TeamMessageBrokerHandler(EventMessageArgs e, FilteredPeopleHolder filteredPeopleHolder)
		{
			_eventMessageArgs = e;
			_filteredPeopleHolder = filteredPeopleHolder;
		}

        private void InsertTeamFromMessageBroker(IRepository<ITeam> repository, Guid id)
        {
            ITeam team = repository.Get(id);

            if (team != null)
            {
            	SiteTeamModel teamExists =
            		_filteredPeopleHolder.SiteTeamAdapterCollection.FirstOrDefault(s => s.Team == team);

                if (teamExists == null)
                {
                    _filteredPeopleHolder.SiteTeamAdapterCollection.Add(EntityConverter.ConvertToOther<ITeam, SiteTeamModel>(team));
                }
            }
        }

        private void DeleteTeamFromMessageBroker(Guid id)
        {
            DeleteContract(id);
        }

        private void UpdateTeamFromMessageBroker(Guid id)
        {
            ITeam team;

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                team = (new TeamRepository(uow)).Get(id);
				if (team!=null)
				{
					LazyLoadingManager.Initialize(team.Site);
				}
            }
            _filteredPeopleHolder.UnitOfWork.Refresh(team);

            if (team != null)
            {
                SiteTeamModel teamExists =
                    _filteredPeopleHolder.SiteTeamAdapterCollection.FirstOrDefault(s => s.Team != null && s.Team.Id == id);

                if (teamExists != null)
                {
                    teamExists.Team = team;
                }

                IList<PersonPeriodModel> adapters =
                    _filteredPeopleHolder.PersonPeriodGridViewCollection.Where(s => s.Team != null && s.Team.Id == id).ToList();

                foreach (PersonPeriodModel adapter in adapters)
                {
                    adapter.Team = team;
                }
            }
        }

        private void DeleteContract(Guid id)
        {
            SiteTeamModel teamExists = _filteredPeopleHolder.SiteTeamAdapterCollection.FirstOrDefault(s => s.Team != null && s.Team.Id == id);

            if (teamExists != null)
            {
                _filteredPeopleHolder.SiteTeamAdapterCollection.Remove(teamExists);
            }
        }

        public void HandleInsertFromMessageBroker()
        {
            InsertTeamFromMessageBroker(new TeamRepository(_filteredPeopleHolder.UnitOfWork),
                                        _eventMessageArgs.Message.DomainObjectId);
        }

        public void HandleDeleteFromMessageBroker()
        {
            DeleteTeamFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
        }
       
        public void HandleUpdateFromMessageBroker()
        {
            UpdateTeamFromMessageBroker(_eventMessageArgs.Message.DomainObjectId);
        }
    }
}