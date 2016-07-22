using System;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class WithReadModelUnitOfWork
	{
		[ReadModelUnitOfWork]
		public virtual void Do(Action action)
		{
			action.Invoke();
		}

		[ReadModelUnitOfWork]
		public virtual T Get<T>(Func<T> func)
		{
			return func.Invoke();
		}
	}
}