using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Context;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class StaticSessionContextBinder : ISessionContextBinder
	{
		private static readonly IDictionary<Guid, NHibernateSessionRelatedData> uowRelatedData
			= new ConcurrentDictionary<Guid, NHibernateSessionRelatedData>(new ConcurrentDictionary<Guid, NHibernateSessionRelatedData>());

		public NHibernateFilterManager FilterManager(ISession session)
		{
			return uowRelatedData[session.GetSessionImplementation().SessionId].FilterManager;
		}

		public TransactionIsolationLevel IsolationLevel(ISession session)
		{
			return uowRelatedData[session.GetSessionImplementation().SessionId].IsolationLevel;
		}

		public void Bind(ISession session, TransactionIsolationLevel isolationLevel)
		{
			uowRelatedData[session.GetSessionImplementation().SessionId] = new NHibernateSessionRelatedData(new NHibernateFilterManager(session), isolationLevel);
			CurrentSessionContext.Bind(session);
		}

		public void Unbind(ISession session)
		{
			CurrentSessionContext.Unbind(session.SessionFactory);
			uowRelatedData.Remove(session.GetSessionImplementation().SessionId);
		}

		public static void UnbindStatic(ISession session)
		{
			CurrentSessionContext.Unbind(session.SessionFactory);
			uowRelatedData.Remove(session.GetSessionImplementation().SessionId);
		}
	}
}