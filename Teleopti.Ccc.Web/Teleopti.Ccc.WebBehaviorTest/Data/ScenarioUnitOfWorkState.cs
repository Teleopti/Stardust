using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class ScenarioUnitOfWorkState
	{
		private static IUnitOfWork _unitOfWork;

		private static IUnitOfWork unitOfWork
		{
			get
			{
				if (_unitOfWork == null)
				{
					_unitOfWork = GlobalUnitOfWorkState.CurrentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
					// might be required for some scenarios, but not right now.
					_unitOfWork.DisableFilter(QueryFilter.BusinessUnit);
				}
				return _unitOfWork;
			}
		}

		public static void DisposeUnitOfWork()
		{
			if (_unitOfWork == null) return;

			unitOfWork.Dispose();
			_unitOfWork = null;
		}

		public static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			action.Invoke(unitOfWork);
			unitOfWork.PersistAll();
		}
	}
}