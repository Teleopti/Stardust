using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class WithUnitOfWork
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public WithUnitOfWork(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[UnitOfWork]
		public virtual void Do(Action action)
		{
			action.Invoke();
		}

		[UnitOfWork]
		public virtual void Do(Action<ICurrentUnitOfWork> action)
		{
			action.Invoke(_unitOfWork);
		}

		[UnitOfWork]
		public virtual T Get<T>(Func<ICurrentUnitOfWork, T> func)
		{
			return func(_unitOfWork);
		}

		[UnitOfWork]
		public virtual T Get<T>(Func<T> func)
		{
			return func();
		}
	}
}