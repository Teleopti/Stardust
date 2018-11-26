using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class JobResultExtensions
	{
		public static JobResult WithBroken(this JobResult jobResult, string brokenType, IPerson agent)
		{
			jobResult.AddDetail(new JobResultDetail(DetailLevel.Info,
				$"{{\"BusinessRulesValidationResults\":[{{\"ResourceId\":\"{agent.Id.Value}\",\"ValidationErrors\":[{{\"ResourceType\":\"{brokenType}\"}}]}}]}}", DateTime.UtcNow, null));
			return jobResult;
		}
	}
}