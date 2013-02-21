using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class ScenarioUnitOfWorkState
	{
		private static IUnitOfWork _unitOfWork;

		public static void OpenUnitOfWork()
		{
			_unitOfWork = GlobalUnitOfWorkState.UnitOfWorkFactory.CreateAndOpenUnitOfWork();
		}

		public static void DisposeUnitOfWork()
		{
			_unitOfWork.Dispose();
			_unitOfWork = null;
		}

		public static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			action.Invoke(_unitOfWork);
			_unitOfWork.PersistAll();
		}

	}
}