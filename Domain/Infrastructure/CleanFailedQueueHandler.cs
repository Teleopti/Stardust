using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class CleanFailedQueueHandler: 
		IHandleEvent<CleanFailedQueue>,
		IRunOnHangfire
	{
		private readonly IHangfireUtilities _hangfireUtilities;

		public CleanFailedQueueHandler(IHangfireUtilities hangfireUtilities)
		{
			_hangfireUtilities = hangfireUtilities;
		}

		[RecurringId("CleanFailedQueueHandler:::CleanFailedQueue")]
		public void Handle(CleanFailedQueue @event)
		{
			_hangfireUtilities.CleanFailedJobsBefore(DateTime.UtcNow - TimeSpan.FromDays(90)); 
		}
	}
}