using System;
using Hangfire.Server;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	// cant be put in the domain yet because of the Hangfire dependency
	public class ActivityChangesChecker : IBackgroundProcess
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

		public void Execute(BackgroundProcessContext context)
		{
			if (_timeout.Equals(TimeSpan.MaxValue))
				return;
			if (!_toggleManager.IsEnabled(Toggles.RTA_ScaleOut_36979))
				_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
			context.CancellationToken.WaitHandle.WaitOne(_timeout);
		}

		public void ExecuteForTest()
		{
			_timeout = TimeSpan.MaxValue;
			if (!_toggleManager.IsEnabled(Toggles.RTA_ScaleOut_36979))
				_tenants.ForAllTenants(t => _rta.CheckForActivityChanges(t));
		}
	}
}