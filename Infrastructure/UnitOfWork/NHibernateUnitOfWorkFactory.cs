using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using NHibernate.Stat;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	/// <summary>
	/// Factory for UnitOfWork. 
	/// Implemented using nhibernate's ISessionFactory.
	/// </summary>
	public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private ISessionFactory _factory;
		private readonly IAuditSetter _auditSettingProvider;
		private bool disposed;

		private readonly IEnumerable<IMessageSender> _messageSenders;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Denormalizers"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			 MessageId = "0")]
		protected internal NHibernateUnitOfWorkFactory(ISessionFactory sessionFactory,
		                                               IAuditSetter auditSettingProvider,
		                                               string connectionString,
		                                               IEnumerable<IMessageSender> messageSenders)
		{
			ConnectionString = connectionString;
			SessionContextBinder = new StaticSessionContextBinder();
			InParameter.NotNull("sessionFactory", sessionFactory);
			sessionFactory.Statistics.IsStatisticsEnabled = true;
			_factory = sessionFactory;
			_auditSettingProvider = auditSettingProvider;
			_messageSenders = messageSenders;
		}

		protected ISessionContextBinder SessionContextBinder { get; set; }

		public string Name
		{
			get { return ((ISessionFactoryImplementor)_factory).Settings.SessionFactoryName; }
		}

		public ISessionFactory SessionFactory
		{
			get { return _factory; }
		}

		public void Close()
		{
			_factory.Close();
		}

		public long? NumberOfLiveUnitOfWorks
		{
			get
			{
				IStatistics statistics = _factory.Statistics;
				if(statistics.IsStatisticsEnabled)
					return statistics.SessionOpenCount - statistics.SessionCloseCount;
				return null;
			}
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			var nhibSession = _factory.OpenStatelessSession();
			return new NHibernateStatelessUnitOfWork(nhibSession);
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			var session = _factory.GetCurrentSession();
			return MakeUnitOfWork(
				session, 
				StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging,
				SessionContextBinder.FilterManager(session)
				);
		}

		public bool HasCurrentUnitOfWork()
		{
			return CurrentSessionContext.HasBind(_factory);
		}

		public IAuditSetter AuditSetting
		{
			get { return _auditSettingProvider; }
		}

		public string ConnectionString { get; private set; }

		protected virtual IUnitOfWork MakeUnitOfWork(ISession session, IMessageBroker messaging, NHibernateFilterManager filterManager)
		{
			return new NHibernateUnitOfWork(session,
			                                messaging,
											_messageSenders,
											filterManager,
											new SendPushMessageWhenRootAlteredService(),
											SessionContextBinder.Unbind
											);
		}

		public virtual IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return CreateAndOpenUnitOfWork(StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IMessageBroker messageBroker)
		{
			var identity = Thread.CurrentPrincipal.Identity as ITeleoptiIdentity;
			var buId = (identity !=null && identity.BusinessUnit!=null) ? identity.BusinessUnit.Id.GetValueOrDefault() : Guid.Empty;
			var interceptor = new AggregateRootInterceptor();
			var nhibSession = createNhibSession(interceptor, buId);

			var nhUow = MakeUnitOfWork(nhibSession, messageBroker, SessionContextBinder.FilterManager(nhibSession));
			return nhUow;
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IAggregateRoot reassociate)
		{
			var uow = CreateAndOpenUnitOfWork();
			uow.Reassociate(reassociate);
			return uow;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		public IUnitOfWork CreateAndOpenUnitOfWork(params IEnumerable<IAggregateRoot>[] reassociate)
		{
			var uow = CreateAndOpenUnitOfWork();
			uow.Reassociate(reassociate);
			return uow;
		}

		private ISession createNhibSession(IInterceptor interceptor, Guid buId)
		{
			ISession nhibSession = _factory.OpenSession(interceptor);
			nhibSession.FlushMode = FlushMode.Never;
			nhibSession.EnableFilter("businessUnitFilter").SetParameter("businessUnitParameter", buId);
			nhibSession.EnableFilter("deletedFlagFilter");
			nhibSession.EnableFilter("deletedPeopleFilter");
			Bind(nhibSession);
			return nhibSession;
		}

		#region Dispose methods

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		public void Dispose()
		{
			Dispose(true);
			//Don't know how to test next row. Impossible?
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					ReleaseManagedResources();
				}
				ReleaseUnmanagedResources();
				disposed = true;
			}
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
			if (_factory != null)
			{
				_factory.Dispose();
				_factory = null;
			}
		}

		#endregion

		internal void Bind(ISession session)
		{
			SessionContextBinder.Bind(session);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal void Unbind(ISession session)
		{
			SessionContextBinder.Unbind(session);
		}

		public static void UnbindStatic(ISession session)
		{
			StaticSessionContextBinder.UnbindStatic(session);
		}

	}

	public interface ISessionContextBinder
	{
		NHibernateFilterManager FilterManager(ISession session);
		void Bind(ISession session);
		void Unbind(ISession session);
	}

	public class StaticSessionContextBinder : ISessionContextBinder
	{
		//todo: byt till ConcurrentDictionary n?r vi uppgraderar till .net 4.0!
		private static readonly IDictionary<Guid, NHibernateSessionRelatedData> uowRelatedData
			 = new ConcurrentDictionary<Guid, NHibernateSessionRelatedData>(new ConcurrentDictionary<Guid, NHibernateSessionRelatedData>());

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public NHibernateFilterManager FilterManager(ISession session)
		{
			return uowRelatedData[session.GetSessionImplementation().SessionId].FilterManager;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Bind(ISession session)
		{
			uowRelatedData[session.GetSessionImplementation().SessionId] = new NHibernateSessionRelatedData(new NHibernateFilterManager(session));
			CurrentSessionContext.Bind(session);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Unbind(ISession session)
		{
			CurrentSessionContext.Unbind(session.SessionFactory);
			uowRelatedData.Remove(session.GetSessionImplementation().SessionId);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void UnbindStatic(ISession session)
		{
			CurrentSessionContext.Unbind(session.SessionFactory);
			uowRelatedData.Remove(session.GetSessionImplementation().SessionId);
		}
	}

}
