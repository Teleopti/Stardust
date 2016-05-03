using System;
using Stardust.Node.Constants;
using Stardust.Node.Entities;

namespace Stardust.Node.Extensions
{
	public static class JobToDoExtensions
	{
		

		public static Uri CreateUri(this JobQueueItemEntity jobQueueItemEntity,
		                            string endPoint)
		{
			var transformUri = new Uri(endPoint.Replace(NodeRouteConstants.JobIdOptionalParameter,
			                                            jobQueueItemEntity.JobId.ToString()));

			return transformUri;
		}

		public static void ThrowExceptionWhenNull(this JobQueueItemEntity jobQueueItemEntity)
		{
			if (jobQueueItemEntity == null)
			{
				throw new ArgumentException();
			}
		}
	}
}