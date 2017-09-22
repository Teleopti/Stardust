using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkAspect : IAnalyticsUnitOfWorkAspect
	{
		private readonly ICurrentAnalyticsUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;

		public AnalyticsUnitOfWorkAspect(ICurrentAnalyticsUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentAnalyticsUnitOfWork unitOfWork)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_unitOfWork = unitOfWork;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			// the uow may be null...
			// .. if the state is disposing
			// .. or something went wrong on before/start
			// .. we think. we have seen exceptions. ;)
			var uow = _unitOfWork.Current();
			try
			{
				if (exception != null) return;
				uow?.PersistAll();
			}
			finally
			{
				uow?.Dispose();
			}
		}
	}
}