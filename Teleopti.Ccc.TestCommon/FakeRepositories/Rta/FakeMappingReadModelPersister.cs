using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReadModelPersister : IMappingReadModelPersister, IMappingReader
	{
		private readonly IList<Mapping> _data = new List<Mapping>();
		private bool _invalido;

		public IEnumerable<Mapping> Data => _data;

		public void Invalidate()
		{
			_invalido = true;
		}

		public bool Invalid()
		{
			return _invalido || _data.IsEmpty();
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			_data.Clear();
			mappings.ForEach(_data.Add);
			_invalido = false;
		}

		public void Add(Mapping mapping)
		{
			_data.Add(mapping);
		}

		public IEnumerable<Mapping> Read()
		{
			return _data;
		}
	}
}