﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReadModelPersister : IMappingReadModelPersister, IMappingReader
	{
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IList<Mapping> _data = new List<Mapping>();

		public IEnumerable<Mapping> Data => _data;

		public FakeMappingReadModelPersister(IKeyValueStorePersister keyValueStore)
		{
			_keyValueStore = keyValueStore;
		}

		public void Invalidate()
		{
			if (Invalid())
				return;
			setInvalido(true);
		}

		private void setInvalido(bool value)
		{
			_keyValueStore.Update("RuleMappingsInvalido", value);
		}

		public bool Invalid()
		{
			return _keyValueStore.Get("RuleMappingsInvalido", true);
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			setInvalido(false);
			_data.Clear();
			mappings.ForEach(_data.Add);
			_keyValueStore.Update("RuleMappingsVersion", Guid.NewGuid().ToString());
		}
		
		public IEnumerable<Mapping> Read()
		{
			return _data;
		}
	}
}