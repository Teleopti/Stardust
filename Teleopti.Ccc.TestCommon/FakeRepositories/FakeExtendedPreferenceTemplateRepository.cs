using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExtendedPreferenceTemplateRepository : IExtendedPreferenceTemplateRepository
	{
		private readonly List<IExtendedPreferenceTemplate> storage = new List<IExtendedPreferenceTemplate>();

		public void Add(IExtendedPreferenceTemplate root)
		{
			storage.Add(root);
		}

		public void Remove(IExtendedPreferenceTemplate root)
		{
			throw new NotImplementedException();
		}

		public IExtendedPreferenceTemplate Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IExtendedPreferenceTemplate Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IExtendedPreferenceTemplate> LoadAll()
		{
			return storage;
		}

		public IList<IExtendedPreferenceTemplate> FindByUser(IPerson user)
		{
			throw new NotImplementedException();
		}

		public IExtendedPreferenceTemplate Find(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}