using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class WithAnalyticsUnitOfWork
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;

		public WithAnalyticsUnitOfWork(ICurrentAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[AnalyticsUnitOfWork]
		public virtual void Do(Action action)
		{
			action.Invoke();
		}

		[AnalyticsUnitOfWork]
		public virtual void Do(Action<ICurrentAnalyticsUnitOfWork> action)
		{
			action.Invoke(_unitOfWork);
		}

		[AnalyticsUnitOfWork]
		public virtual T Get<T>(Func<T> func)
		{
			return func.Invoke();
		}

		[AnalyticsUnitOfWork]
		public virtual T Get<T>(Func<ICurrentAnalyticsUnitOfWork, T> func)
		{
			return func.Invoke(_unitOfWork);
		}
	}
}