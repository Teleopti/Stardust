using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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