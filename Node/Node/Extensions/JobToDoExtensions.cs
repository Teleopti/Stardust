using System;
using Newtonsoft.Json;
using Stardust.Node.Constants;
using Stardust.Node.Entities;

namespace Stardust.Node.Extensions
{
	public static class JobToDoExtensions
	{
		public static string SerializeToJson(this JobQueueItemEntity jobQueueItemEntity)
		{
			jobQueueItemEntity.ThrowExceptionWhenNull();

			return JsonConvert.SerializeObject(jobQueueItemEntity);
		}

		public static Uri CreateUri(this JobQueueItemEntity jobQueueItemEntity,
		                            string endPoint)
		{
			jobQueueItemEntity.ThrowExceptionWhenNull();
			endPoint.ThrowArgumentExceptionIfNullOrEmpty();

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