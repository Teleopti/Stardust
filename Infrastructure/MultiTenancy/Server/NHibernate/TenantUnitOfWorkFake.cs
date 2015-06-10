﻿using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkFake : ITenantUnitOfWork
	{
		private Exception _exceptionToThrow;

		public void CancelAndDisposeCurrent()
		{
		}

		public void CommitAndDisposeCurrent()
		{
			if(_exceptionToThrow!=null)
				throw _exceptionToThrow;

			WasCommitted = true;
		}

		public bool WasCommitted { get; private set; }

		public void WillThrowAtCommit(Exception exceptionToThrow)
		{
			_exceptionToThrow = exceptionToThrow;
		}
	}
}