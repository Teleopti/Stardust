using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData;
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
					_unitOfWork = GlobalUnitOfWorkState.CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
					_unitOfWork.DisableFilter(QueryFilter.BusinessUnit);
				}
				return _unitOfWork;
			}
		}

		public static void TryDisposeUnitOfWork()
		{
			if (_unitOfWork == null) return;

			unitOfWork.Dispose();
			_unitOfWork = null;
		}

		public static void UnitOfWorkAction(Action<ICurrentUnitOfWork> action)
		{
			action.Invoke(new ThisUnitOfWork(unitOfWork));
			unitOfWork.PersistAll();
		}
	}
}