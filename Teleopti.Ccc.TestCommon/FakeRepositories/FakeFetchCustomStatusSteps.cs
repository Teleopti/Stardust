using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeFetchCustomStatusSteps : IFetchCustomStatusSteps
	{
		private readonly IList<CustomStatusStep> _statusSteps = new List<CustomStatusStep>();

		public void Has(CustomStatusStep statusStep)
		{
			_statusSteps.Add(statusStep);
		}
		
		public IEnumerable<CustomStatusStep> Execute()
		{
			return _statusSteps;
		}

		public CustomStatusStep Execute(string name)
		{
			return _statusSteps.SingleOrDefault(x => x.Name == name);
		}
	}
}