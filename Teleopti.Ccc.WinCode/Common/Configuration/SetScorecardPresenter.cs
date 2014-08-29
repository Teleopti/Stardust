using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class SetScorecardPresenter : IDisposable, IInitiatorIdentifier
    {
        private readonly ISetScorecardView _view;
        private readonly IUnitOfWork _unitOfWork;
		private readonly IMessageListener _messageBroker;
        private readonly IScorecardProvider _scorecardProvider;
        private readonly ISiteProvider _siteProvider;
        private readonly ITeamProvider _teamProvider;
        private readonly Guid _messageBrokerInstance = Guid.NewGuid();
        private ISite _selectedSite;

        public SetScorecardPresenter(ISetScorecardView view, IUnitOfWork unitOfWork, IMessageListener messageBroker, IScorecardProvider scorecardProvider, ISiteProvider siteProvider, ITeamProvider teamProvider)
        {
            _view = view;
            _unitOfWork = unitOfWork;
            _messageBroker = messageBroker;
            _scorecardProvider = scorecardProvider;
            _siteProvider = siteProvider;
            _teamProvider = teamProvider;
        }

        public Guid InitiatorId
        {
            get { return _messageBrokerInstance; }
        }

        public void SaveChanges()
        {
            _unitOfWork.PersistAll(this);
        }

        public void Initialize()
        {
            RefreshScorecards();
            
            var sites = _siteProvider.GetSites();
            _view.SetSites(sites);
            _view.SetSelectedSite(sites[0]);

            _messageBroker.RegisterEventSubscription(OnTeamEvent,typeof(ITeam));
            _messageBroker.RegisterEventSubscription(OnSiteEvent, typeof(ISite));
            _messageBroker.RegisterEventSubscription(OnScorecardEvent, typeof(IScorecard));
        }

        private void RefreshScorecards()
        {
            _view.SetScorecards(_scorecardProvider.GetScorecards());
        }

        public void SelectSite(ISite site)
        {
            _selectedSite = site;
            List<ITeamScorecardModel> teams = new List<ITeamScorecardModel>();
            var allTeams = _teamProvider.GetTeams();
            if (!_siteProvider.AllSitesItem.Equals(site))
            {
                allTeams = allTeams.Where(t => site.Equals(t.Site));
            }
            teams.AddRange(allTeams.Select(t => (ITeamScorecardModel)new TeamScorecardModel(t)));
            _view.SetTeams(teams);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();

            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            if (_messageBroker != null)
            {
                _messageBroker.UnregisterSubscription(OnTeamEvent);
                _messageBroker.UnregisterSubscription(OnSiteEvent);
                _messageBroker.UnregisterSubscription(OnScorecardEvent);
            }
        }

        private void OnScorecardEvent(object sender, EventMessageArgs e)
        {
            if (e.Message.ModuleId == InitiatorId) return;
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new EventHandler<EventMessageArgs>(OnScorecardEvent),sender,e);
            }
            else
            {
                _scorecardProvider.HandleMessageBrokerEvent(e.Message.DomainObjectId, e.Message.DomainUpdateType);
                _view.SetScorecards(_scorecardProvider.GetScorecards());
            }
        }

        private void OnSiteEvent(object sender, EventMessageArgs e)
        {
            if (e.Message.ModuleId==InitiatorId) return;
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new EventHandler<EventMessageArgs>(OnSiteEvent), sender, e);
            }
            else
            {
                _siteProvider.HandleMessageBrokerEvent(e.Message.DomainObjectId, e.Message.DomainUpdateType);
                var sites = _siteProvider.GetSites();
                _view.SetSites(sites);
                SelectedSameSiteAgainIfPossible(sites);
            }
        }

        private void OnTeamEvent(object sender, EventMessageArgs e)
        {
            if (e.Message.ModuleId == InitiatorId) return;
            if (_view.InvokeRequired)
            {
                _view.BeginInvoke(new EventHandler<EventMessageArgs>(OnTeamEvent), sender, e);
            }
            else
            {
                _teamProvider.HandleMessageBrokerEvent(e.Message.DomainObjectId, e.Message.DomainUpdateType);
                var sites = _siteProvider.GetSites();
                SelectedSameSiteAgainIfPossible(sites);
            }
        }

        private void SelectedSameSiteAgainIfPossible(IList<ISite> sites)
        {
            ISite siteToShow = _selectedSite;
            if (!sites.Contains(_selectedSite))
            {
                siteToShow = sites[0];
            }
            _view.SetSelectedSite(siteToShow);
        }
    }
}