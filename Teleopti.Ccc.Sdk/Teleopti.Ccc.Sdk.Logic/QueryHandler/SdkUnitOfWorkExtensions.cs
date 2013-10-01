using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	internal static class SdkUnitOfWorkExtensions
	{
		internal static IDisposable LoadDeletedIfSpecified(this IUnitOfWork unitOfWork, bool shouldLoadDeleted)
		{
			return shouldLoadDeleted ? unitOfWork.DisableFilter(QueryFilter.Deleted) : new emptyDisposable();
		}

		private class emptyDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}