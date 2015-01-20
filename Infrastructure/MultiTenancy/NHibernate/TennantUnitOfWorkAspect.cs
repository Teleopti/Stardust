using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class TennantUnitOfWorkAspect : IAspect
	{
		private readonly ITennantSessionFactory _tennantSessionFactory;
		private readonly ICurrentTennantSession _currentTennantSession;

		public TennantUnitOfWorkAspect(ITennantSessionFactory tennantSessionFactory, ICurrentTennantSession currentTennantSession)
		{
			_tennantSessionFactory = tennantSessionFactory;
			_currentTennantSession = currentTennantSession;
		}

		public void OnBeforeInvokation()
		{
			_tennantSessionFactory.StartTransaction();
		}

		public void OnAfterInvokation(Exception exception)
		{
			var session = _currentTennantSession.Session();
			if (exception == null)
			{
				session.Transaction.Commit();
			}
			session.Dispose();
			_tennantSessionFactory.EndTransaction();
		}
	}
}