﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReadModelPersister : IMappingReadModelPersister
	{
		private readonly IList<Mapping> _data = new List<Mapping>();

		public IEnumerable<Mapping> Data {get { return _data; }}

		public void Invalidate()
		{
		}

		public bool Invalid()
		{
			return _invalido || _data.IsEmpty();
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			_data.Clear();
			mappings.ForEach(_data.Add);
		}

		public void Add(Mapping mapping)
		{
			_data.Add(mapping);
		}
	}
}