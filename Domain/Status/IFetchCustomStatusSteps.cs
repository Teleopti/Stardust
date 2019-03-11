using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Status
{
	public interface IFetchCustomStatusSteps
	{
		IEnumerable<IStatusStep> Execute();
	}
}