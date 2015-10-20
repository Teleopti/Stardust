using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
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

		public void Do(Action<IUnitOfWork> action)
		{
			using (var uow = _factory.Current().CreateAndOpenUnitOfWork())
			{
				action.Invoke(uow);
				uow.PersistAll();
			}
		}

		public T Get<T>(Func<T> func)
		{
			using (_factory.Current().CreateAndOpenUnitOfWork()) return func();
		}
	}
}