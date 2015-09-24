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
		private readonly INow _now;
		private readonly ManualResetEvent _hasExecuted = new ManualResetEvent(false);
		private readonly ManualResetEvent _forceExecute = new ManualResetEvent(false);

		public ActivityChangesChecker(
			Domain.ApplicationLayer.Rta.Service.Rta rta, 
			RtaTenants tenants,
			INow now
			)
		{
			_rta = rta;
			_tenants = tenants;
			_now = now;
		}

		public void Execute(CancellationToken cancellationToken)
		{
			_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
			_hasExecuted.Set();
			waitForNextRun(cancellationToken);
		}

		// this method ensure the server component will run when simulating the time moving forward
		// test-induced damange
		private void waitForNextRun(CancellationToken cancellationToken)
		{
			var nextRun = _now.UtcDateTime().AddMinutes(1);
			while (!cancellationToken.IsCancellationRequested)
			{
				if (_forceExecute.WaitOne(0))
				{
					_forceExecute.Reset();
					break;
				}
				if (_now.UtcDateTime() >= nextRun)
					break;
				cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));
			}
		}

		// test-induced damange
		public void WaitForOneExecution()
		{
			_hasExecuted.Reset();
			_forceExecute.Set();
			_hasExecuted.WaitOne();
		}
	}
}