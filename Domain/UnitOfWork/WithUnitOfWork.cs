using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.UnitOfWork
{
	public class WithUnitOfWork
	{
		private readonly ICurrentUnitOfWorkFactory _factory;

		public WithUnitOfWork(ICurrentUnitOfWorkFactory factory)
		{
			_factory = factory;
		}

		public void Do(Action action)
		{
			using (var uow = _factory.Current().CreateAndOpenUnitOfWork())
			{
				action.Invoke();
				uow.PersistAll();
			}
		}

		public void Do(Action<ICurrentUnitOfWork> action)
		{
			using (var uow = _factory.Current().CreateAndOpenUnitOfWork())
			{
				action.Invoke(new ThisUnitOfWork(uow));
				uow.PersistAll();
			}
		}

		public T Get<T>(Func<ICurrentUnitOfWork, T> func)
		{
			using (var uow = _factory.Current().CreateAndOpenUnitOfWork())
			{
				return func(new ThisUnitOfWork(uow));
			}
		}

		public T Get<T>(Func<T> func)
		{
			using (_factory.Current().CreateAndOpenUnitOfWork()) return func();
		}
	}
}