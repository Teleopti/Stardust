using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Server;
using Hangfire.SqlServer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	// with Hangfire 1.5, we think we can inject our server component without deriving from SqlServerStorage
	public class SqlStorageWithActivityChangesCheckerComponent : SqlServerStorage
	{
		private readonly ActivityChangesChecker _activityChangesChecker;

		public SqlStorageWithActivityChangesCheckerComponent(ActivityChangesChecker activityChangesChecker, string nameOrConnectionString, SqlServerStorageOptions options) : base(nameOrConnectionString, options)
		{
			_activityChangesChecker = activityChangesChecker;
		}

		public override IEnumerable<IServerComponent> GetComponents()
		{
			return base.GetComponents()
				.Union(new[] { _activityChangesChecker });

		}
	}

	// cant be put in the domain yet because of the Hangfire dependency
	public class ActivityChangesChecker : IServerComponent
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private readonly TenantsInitializedInRta _tenants;
		private TimeSpan _timeout = TimeSpan.FromMinutes(1);

		public ActivityChangesChecker(
			Domain.ApplicationLayer.Rta.Service.Rta rta, 
			TenantsInitializedInRta tenants
			)
		{
			_rta = rta;
			_tenants = tenants;
		}

		public void Execute(CancellationToken cancellationToken)
		{
			if (_timeout.Equals(TimeSpan.MaxValue))
				return;
			_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
			cancellationToken.WaitHandle.WaitOne(_timeout);
		}

		public void ExecuteForTest()
		{
			_timeout = TimeSpan.MaxValue;
			_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
		}
	}
}