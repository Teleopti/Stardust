using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class GlobalUnitOfWorkState
	{
		public static NHibernateUnitOfWorkFactory UnitOfWorkFactory;

		public static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			using (var unitOfWork = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				action.Invoke(unitOfWork);
				unitOfWork.PersistAll();
			}
		}

	}
}