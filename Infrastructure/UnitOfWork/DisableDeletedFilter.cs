﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DisableDeletedFilter : IDisableDeletedFilter
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public DisableDeletedFilter(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IDisposable Disable()
		{
			return _currentUnitOfWork.Current().DisableFilter(QueryFilter.Deleted);
		}
	}
}