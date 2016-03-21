using System;
using Newtonsoft.Json;
using Stardust.Node.Constants;
using Stardust.Node.Entities;

namespace Stardust.Node.Extensions
{
	public static class JobToDoExtensions
	{
		public static string SerializeToJson(this JobToDo jobToDo)
		{
			jobToDo.ThrowExceptionWhenNull();

			return JsonConvert.SerializeObject(jobToDo);
		}

		public static Uri CreateUri(this JobToDo jobToDo,
		                            string endPoint)
		{
			jobToDo.ThrowExceptionWhenNull();
			endPoint.ThrowArgumentExceptionIfNullOrEmpty();

			var transformUri = new Uri(endPoint.Replace(NodeRouteConstants.JobIdOptionalParameter,
			                                            jobToDo.Id.ToString()));

			return transformUri;
		}

		public static void ThrowExceptionWhenNull(this JobToDo jobToDo)
		{
			if (jobToDo == null)
			{
				throw new ArgumentException();
			}
		}
	}
}