using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
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

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var factory = _currentDataSource.Current().ReadModel;
			factory.StartUnitOfWork();
		}

		public void OnAfterInvocation(Exception exception)
		{
			var factory = _currentDataSource.Current().ReadModel;
			factory.EndUnitOfWork(exception);
		}
	}
}