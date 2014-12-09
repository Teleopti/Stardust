﻿using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAspect : IReadModelUnitOfWorkAspect
	{
		private readonly ICurrentDataSource _currentDataSource;

		public ReadModelUnitOfWorkAspect(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public void OnBeforeInvokation()
		{
			var factory = _currentDataSource.Current().ReadModel;
			factory.StartUnitOfWork();
		}

		public void OnAfterInvokation(Exception exception)
		{
			var factory = _currentDataSource.Current().ReadModel;
			factory.EndUnitOfWork(exception);
		}
	}

	public class ReadModelUpdatedMessage
	{
	}
}