using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class GlobalUnitOfWorkState
	{
		public static ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		public static void UnitOfWorkAction(Action<ICurrentUnitOfWork> action)
		{
			using (var unitOfWork = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				action.Invoke(new ThisUnitOfWork(unitOfWork));
				unitOfWork.PersistAll();
			}
		}
	}
}