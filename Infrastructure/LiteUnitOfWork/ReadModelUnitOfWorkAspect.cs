using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAspect : IReadModelUnitOfWorkAspect
	{
		private readonly IAvailableDataSourcesProvider _availableDataSourcesProvider;

		public ReadModelUnitOfWorkAspect(IAvailableDataSourcesProvider availableDataSourcesProvider)
		{
			_availableDataSourcesProvider = availableDataSourcesProvider;
		}

		public void OnBeforeInvokation()
		{
			var factory = _availableDataSourcesProvider.AvailableDataSources().FirstOrDefault().ReadModel;
			factory.StartUnitOfWork();
		}

		public void OnAfterInvokation(Exception exception)
		{
			var factory = _availableDataSourcesProvider.AvailableDataSources().FirstOrDefault().ReadModel;
			factory.EndUnitOfWork(exception);
		}
	}
}