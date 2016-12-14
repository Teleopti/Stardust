using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy = null;
		private static IUpdatedBy _updatedBy;

		public static INestedUnitOfWorkStrategy NestedUnitOfWorkStrategy
		{
			get
			{
				var nestedUnitOfWorkStrategy = _nestedUnitOfWorkStrategy ?? new ThrowOnNestedUnitOfWork();
				Console.WriteLine("GET" + nestedUnitOfWorkStrategy.GetType().Name);
				return nestedUnitOfWorkStrategy;
			}
			set
			{
				Console.WriteLine($"SET {value?.GetType().Name} {new StackTrace().ToString()}");
				_nestedUnitOfWorkStrategy = value;
			}
		}

		public static IUpdatedBy UpdatedBy
		{
			get { return _updatedBy ?? Domain.Security.Principal.UpdatedBy.Make(); }
			set { _updatedBy = value; }
		}

	}

}
