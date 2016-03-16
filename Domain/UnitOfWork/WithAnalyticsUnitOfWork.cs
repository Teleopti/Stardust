using System;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.UnitOfWork
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