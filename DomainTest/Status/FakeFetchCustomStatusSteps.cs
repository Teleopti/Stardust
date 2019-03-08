using System.Collections.Generic;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.DomainTest.Status
{
	public class FakeFetchCustomStatusSteps : IFetchCustomStatusSteps
	{
		private readonly IList<CustomStatusStep> _statusSteps = new List<CustomStatusStep>();

		public void Has(CustomStatusStep statusStep)
		{
			_statusSteps.Add(statusStep);
		}
		
		public IEnumerable<IStatusStep> Execute()
		{
			return _statusSteps;
		}
	}
}