using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Status
{
	public interface IFetchCustomStatusSteps
	{
		IEnumerable<CustomStatusStep> Execute();
		CustomStatusStep Execute(string name);
	}
}