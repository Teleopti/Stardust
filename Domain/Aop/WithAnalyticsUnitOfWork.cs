using System;

namespace Teleopti.Ccc.Domain.Aop
{
	public class WithAnalyticsUnitOfWork
	{
		[AnalyticsUnitOfWork]
		public virtual void Do(Action action)
		{
			action.Invoke();
		}

		[AnalyticsUnitOfWork]
		public virtual T Get<T>(Func<T> func)
		{
			return func.Invoke();
		}
	}
}