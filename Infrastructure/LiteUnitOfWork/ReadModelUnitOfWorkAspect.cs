using System;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAspect : IReadModelUnitOfWorkAspect
	{
		private readonly ReadModelUnitOfWorkFactory _factory;

		public ReadModelUnitOfWorkAspect(ReadModelUnitOfWorkFactory factory)
		{
			_factory = factory;
		}

		public void OnBeforeInvokation()
		{
			_factory.StartUnitOfWork();
		}

		public void OnAfterInvokation(Exception exception)
		{
			_factory.EndUnitOfWork(exception);
		}
	}
}