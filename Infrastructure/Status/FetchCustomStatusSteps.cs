using System.Collections.Generic;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.Infrastructure.Status
{
	public class FetchCustomStatusSteps : IFetchCustomStatusSteps
	{
		public IEnumerable<IStatusStep> Execute()
		{
			yield break;
		}
	}
}