using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class CleanFailedQueueHandler: 
		IHandleEvent<CleanFailedQueue>,
		IRunOnHangfire
	{
		private readonly ICleanHangfire _hangfireUtilities;
		private readonly IConfigReader _config;

		public CleanFailedQueueHandler(ICleanHangfire hangfireUtilities, IConfigReader config)
		{
			_hangfireUtilities = hangfireUtilities;
			_config = config;
		}

		public void Handle(CleanFailedQueue @event)
		{
			var expiryDays = _config.ReadValue("HangfireCleanFailedJobsAfterDays", 90);
			_hangfireUtilities.CleanFailedJobsBefore(DateTime.UtcNow - TimeSpan.FromDays(expiryDays));
		}
	}

	public interface ICleanHangfire
	{
		void CleanFailedJobsBefore(DateTime time);
	}
}