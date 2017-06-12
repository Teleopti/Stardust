using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePlanningGroupRepository : IPlanningGroupRepository
	{
		private readonly List<IPlanningGroup> _planningGroups = new List<IPlanningGroup>();

		public void Add(IPlanningGroup root)
		{
			_planningGroups.Add(root); // Should set Id
		}

		public void Remove(IPlanningGroup root)
		{
			((IDeleteTag)Get(root.Id.GetValueOrDefault())).SetDeleted();
		}

		public IPlanningGroup Get(Guid id)
		{
			return _planningGroups.FirstOrDefault(x => x.Id == id);
		}

		public IPlanningGroup Load(Guid id)
		{
			return _planningGroups.First(x => x.Id == id);
		}

		public IList<IPlanningGroup> LoadAll()
		{
			return _planningGroups;
		}

		public FakePlanningGroupRepository Has(IPlanningGroup root)
		{
			_planningGroups.Add(root);
			return this;
		}
	}
}