using System;
using Hangfire.Server;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class RecurringEventPublishingUpdater : IBackgroundProcess
	{
		private readonly RecurringEventPublishings _publishings;

		public RecurringEventPublishingUpdater(RecurringEventPublishings publishings)
		{
			_publishings = publishings;
		}
		
		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				_publishings.UpdatePublishings();
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(10));
			}
			finally
			{
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
			}
		}
	}
}