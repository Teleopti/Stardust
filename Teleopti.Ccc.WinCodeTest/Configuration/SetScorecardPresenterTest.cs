using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class SetScorecardPresenterTest : IDisposable
    {
        private MockRepository _mocks;
        private IUnitOfWork _unitOfWork;
        private ITeamProvider _teamProvider;
        private IScorecardProvider _scorecardProvider;
        private ISetScorecardView _view;
        private SetScorecardPresenter _target;
        private ISiteProvider _siteProvider;
        private IMessageBrokerListener _messageBroker;
        private MethodInfo _onSiteEvent;
        private MethodInfo _onScorecardEvent;
        private MethodInfo _onTeamEvent;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            _teamProvider = _mocks.StrictMock<ITeamProvider>();
            _scorecardProvider = _mocks.StrictMock<IScorecardProvider>();
            _siteProvider = _mocks.StrictMock<ISiteProvider>();
            _view = _mocks.StrictMock<ISetScorecardView>();
            _messageBroker = _mocks.StrictMock<IMessageBrokerListener>();
            _target = new SetScorecardPresenter(_view, _unitOfWork, _messageBroker, _scorecardProvider, _siteProvider, _teamProvider);

            _onSiteEvent = _target.GetType().GetMethod("OnSiteEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            _onTeamEvent = _target.GetType().GetMethod("OnTeamEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            _onScorecardEvent = _target.GetType().GetMethod("OnScorecardEvent", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [Test]
        public void VerifyInitialize()
        {
        	var identity = (ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
            var site = _mocks.StrictMock<ISite>();
            var sites = new List<ISite> { site};
            var scorecards = new List<IScorecard> {_mocks.StrictMock<IScorecard>()};
            using (_mocks.Record())
            {
                Expect.Call(_scorecardProvider.GetScorecards()).Return(scorecards);
                Expect.Call(_siteProvider.GetSites()).Return(sites);
                Expect.Call(() => _view.SetSites(sites));
                Expect.Call(() => _view.SetScorecards(scorecards));
                Expect.Call(() => _view.SetSelectedSite(site));
                Expect.Call(
                    () =>
                    _messageBroker.RegisterEventSubscription(
						identity.DataSource.DataSourceName,identity.BusinessUnit.Id.GetValueOrDefault(),
                        (EventHandler<EventMessageArgs>)
                        Delegate.CreateDelegate(typeof (EventHandler<EventMessageArgs>), _target, _onScorecardEvent),
                        typeof (IScorecard)));
                Expect.Call(
                    () =>
                    _messageBroker.RegisterEventSubscription(
					identity.DataSource.DataSourceName, identity.BusinessUnit.Id.GetValueOrDefault(),
                        (EventHandler<EventMessageArgs>)
                        Delegate.CreateDelegate(typeof (EventHandler<EventMessageArgs>), _target, _onTeamEvent),
                        typeof (ITeam)));
                Expect.Call(
                    () =>
                    _messageBroker.RegisterEventSubscription(
					identity.DataSource.DataSourceName, identity.BusinessUnit.Id.GetValueOrDefault(),
                        (EventHandler<EventMessageArgs>)
                        Delegate.CreateDelegate(typeof (EventHandler<EventMessageArgs>), _target, _onSiteEvent),
                        typeof (ISite)));
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test]
        public void VerifySelectSite()
        {
            var site = _mocks.StrictMock<ISite>();
            var allSitesSite = _mocks.StrictMock<ISite>();
            var team = _mocks.StrictMock<ITeam>();
            var teams = new List<ITeam> { team };
            using (_mocks.Record())
            {
                Expect.Call(_siteProvider.AllSitesItem).Return(allSitesSite);
                Expect.Call(_teamProvider.GetTeams()).Return(teams);
                Expect.Call(team.Site).Return(site);
                Expect.Call(() => _view.SetTeams(new List<ITeamScorecardModel>())).IgnoreArguments();
                Expect.Call(site.Equals(site)).Return(true);
                Expect.Call(allSitesSite.Equals(site)).Return(false);
            }
            using (_mocks.Playback())
            {
                _target.SelectSite(site);
            }
        }

        [Test]
        public void VerifySelectAllSites()
        {
            var site = _mocks.StrictMock<ISite>();
            var team = _mocks.StrictMock<ITeam>();
            var teams = new List<ITeam> {team};
            using (_mocks.Record())
            {
                Expect.Call(_siteProvider.AllSitesItem).Return(site);
                Expect.Call(_teamProvider.GetTeams()).Return(teams);
                Expect.Call(()=>_view.SetTeams(new List<ITeamScorecardModel>())).IgnoreArguments();
                Expect.Call(site.Equals(site)).Return(true);
            }
            using (_mocks.Playback())
            {
                _target.SelectSite(site);
            }
        }

        [Test]
        public void VerifyCanSaveChanges()
        {
            using(_mocks.Record())
            {
                Expect.Call(_unitOfWork.PersistAll(_target)).Return(null);
            }
            using (_mocks.Playback())
            {
                _target.SaveChanges();
            }
        }

        private EventMessageArgs CreateMessage(bool sameModule)
        {
            var eventMessage = _mocks.StrictMock<IEventMessage>();
            if (sameModule)
            {
                Expect.Call(eventMessage.ModuleId).Return(_target.InitiatorId);
            }
            else
            {
                Expect.Call(eventMessage.ModuleId).Return(Guid.NewGuid());
                Expect.Call(eventMessage.DomainObjectId).Return(Guid.Empty);
                Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Insert);
            }
            
            var eventMessageArgs = new EventMessageArgs(eventMessage);
            return eventMessageArgs;
        }

        [Test]
        public void VerifyCanHandleIncomingSiteMessage()
        {
            var site = _mocks.StrictMock<ISite>();
            var sites = new List<ISite> { site};
            
            EventMessageArgs message;
            using (_mocks.Record())
            {
                message = CreateMessage(false);
                Expect.Call(_view.InvokeRequired).Return(false);
                Expect.Call(() => _siteProvider.HandleMessageBrokerEvent(Guid.Empty, DomainUpdateType.Insert));
                Expect.Call(_siteProvider.GetSites()).Return(sites);
                Expect.Call(() => _view.SetSites(sites));
                Expect.Call(() => _view.SetSelectedSite(site));
            }
            using (_mocks.Playback())
            {
                _onSiteEvent.Invoke(_target, new object[] {null, message});
            }
        }

        [Test]
        public void VerifyCanHandleIncomingTeamMessage()
        {
            var site = _mocks.StrictMock<ISite>();
            EventMessageArgs message;
            using (_mocks.Record())
            {
                message = CreateMessage(false);
                Expect.Call(_view.InvokeRequired).Return(false);
                Expect.Call(() => _teamProvider.HandleMessageBrokerEvent(Guid.Empty, DomainUpdateType.Insert));
                Expect.Call(_siteProvider.GetSites()).Return(new List<ISite> {site});
                Expect.Call(() => _view.SetSelectedSite(site));
            }
            using (_mocks.Playback())
            {
                _onTeamEvent.Invoke(_target, new object[] { null, message });
            }
        }

        [Test]
        public void VerifyCanHandleIncomingScorecardMessage()
        {
            var scorecards = new List<IScorecard>();

            EventMessageArgs message;
            using (_mocks.Record())
            {
                message = CreateMessage(false);
                Expect.Call(_view.InvokeRequired).Return(false);
                Expect.Call(() => _scorecardProvider.HandleMessageBrokerEvent(Guid.Empty, DomainUpdateType.Insert));
                Expect.Call(_scorecardProvider.GetScorecards()).Return(scorecards);
                Expect.Call(() => _view.SetScorecards(scorecards));
            }
            using (_mocks.Playback())
            {
                _onScorecardEvent.Invoke(_target, new object[] { null, message });
            }
        }

        [Test]
        public void VerifyDispose()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _messageBroker.UnregisterEventSubscription((EventHandler<EventMessageArgs>)
                        Delegate.CreateDelegate(typeof(EventHandler<EventMessageArgs>), _target, _onScorecardEvent)));
                Expect.Call(() => _messageBroker.UnregisterEventSubscription((EventHandler<EventMessageArgs>)
                        Delegate.CreateDelegate(typeof(EventHandler<EventMessageArgs>), _target, _onSiteEvent)));
                Expect.Call(() => _messageBroker.UnregisterEventSubscription((EventHandler<EventMessageArgs>)
                        Delegate.CreateDelegate(typeof(EventHandler<EventMessageArgs>), _target, _onTeamEvent)));
            }
            using (_mocks.Playback())
            {
                _target.Dispose();
            }
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
            if (_target!=null)
            {
                _mocks.BackToRecord(_messageBroker);
                using (_mocks.Record())
                {
                    Expect.Call(()=> _messageBroker.UnregisterEventSubscription(null)).IgnoreArguments().Repeat.Times(3);
                }
                using (_mocks.Playback())
                {
                    _target.Dispose();
                }
            }
        }
    }
}
