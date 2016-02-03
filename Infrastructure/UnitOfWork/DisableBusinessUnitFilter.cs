using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DisableBusinessUnitFilter : IDisableBusinessUnitFilter
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public DisableBusinessUnitFilter(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IDisposable Disable()
		{
			return _currentUnitOfWork.Current().DisableFilter(QueryFilter.BusinessUnit);
		}
	}
}