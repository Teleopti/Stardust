using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Interfaces.Infrastructure;

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
			var unitOfWork = _unitOfWork.Current();
			unitOfWork.PersistAll();
			unitOfWork.Dispose();
		}
	}
}