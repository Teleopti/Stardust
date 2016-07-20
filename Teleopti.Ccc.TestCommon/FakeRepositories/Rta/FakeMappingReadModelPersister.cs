using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReadModelPersister : IMappingReadModelPersister
	{
		private readonly IList<Mapping> _data = new List<Mapping>();

		public IEnumerable<Mapping> Data {get { return _data; }}

		public void Invalidate()
		{
		}

		public void Add(Mapping mapping)
		{
			_data.Add(mapping);
		}
	}
}