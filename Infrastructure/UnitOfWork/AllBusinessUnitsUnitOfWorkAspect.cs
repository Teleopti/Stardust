using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class AllBusinessUnitsUnitOfWorkAspect : IAllBusinessUnitsUnitOfWorkAspect
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IUnitOfWork _unitOfWork;

		public AllBusinessUnitsUnitOfWorkAspect(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void OnBeforeInvocation()
		{
			_unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork(QueryFilter.NoFilter);
		}

		public void OnAfterInvocation(Exception exception)
		{
			_unitOfWork.Dispose();
		}
	}
}