using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Server;
using Hangfire.SqlServer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

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
		private readonly IToggleManager _toggleManager;
		private TimeSpan _timeout = TimeSpan.FromMinutes(1);

		public ActivityChangesChecker(
			Domain.ApplicationLayer.Rta.Service.Rta rta, 
			TenantsInitializedInRta tenants,
			IToggleManager toggleManager
			)
		{
			_rta = rta;
			_tenants = tenants;
			_toggleManager = toggleManager;
		}

		public void Execute(CancellationToken cancellationToken)
		{
			if (_timeout.Equals(TimeSpan.MaxValue))
				return;
			if (!_toggleManager.IsEnabled(Toggles.RTA_ScaleOut_36979))
				_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
			cancellationToken.WaitHandle.WaitOne(_timeout);
		}

		public void ExecuteForTest()
		{
			_timeout = TimeSpan.MaxValue;
			if (!_toggleManager.IsEnabled(Toggles.RTA_ScaleOut_36979))
				_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
		}
	}
}