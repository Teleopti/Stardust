using System;
using Teleopti.Ccc.IocCommon.Aop.Core;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkAspect : IAspect
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