using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire.Server;
using Hangfire.SqlServer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Hangfire
{

	// with Hangfire 1.5, we think we can inject our server component without deriving from SqlServerStorage
	[CLSCompliant(false)]
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
		private readonly RtaTenants _tenants;

		public ActivityChangesChecker(
			Domain.ApplicationLayer.Rta.Service.Rta rta, 
			RtaTenants tenants
			)
		{
			_rta = rta;
			_tenants = tenants;
		}

		public void Execute(CancellationToken cancellationToken)
		{
			_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
			cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
		}

		public void ExecuteForTest()
		{
			_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
		}
	}
}