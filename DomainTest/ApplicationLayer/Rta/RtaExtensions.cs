using System;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public static class RtaExtensions
	{
		public static void ReloadAndCheckForActivityChanges(this Domain.ApplicationLayer.Rta.Service.Rta rta, string tenant, Guid personId)
		{
			rta.ReloadSchedulesOnNextCheckForActivityChanges(tenant, personId);
			rta.CheckForActivityChanges(tenant);
		}
	}
}