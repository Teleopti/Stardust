using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData
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

	public class GlobalDataMaker
	{
		private static readonly DataFactory _dataFactory = new DataFactory(GlobalUnitOfWorkState.UnitOfWorkAction);

		public static DataFactory Data() { return _dataFactory; }

	}
}