using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class GlobalUnitOfWorkState
	{
		public static IUnitOfWorkFactoryProvider UnitOfWorkFactoryProvider;

		public static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			using (var unitOfWork = UnitOfWorkFactoryProvider.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				action.Invoke(unitOfWork);
				unitOfWork.PersistAll();
			}
		}
	}
}