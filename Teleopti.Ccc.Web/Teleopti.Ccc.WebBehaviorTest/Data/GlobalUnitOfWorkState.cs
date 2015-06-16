using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class GlobalUnitOfWorkState
	{
		public static ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		public static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			using (var unitOfWork = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				action.Invoke(unitOfWork);
				unitOfWork.PersistAll();
			}
		}
	}
}