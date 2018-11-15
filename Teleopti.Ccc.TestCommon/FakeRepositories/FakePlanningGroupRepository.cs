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
		private readonly List<PlanningGroup> _planningGroups = new List<PlanningGroup>();

		public void Add(PlanningGroup root)
		{
			_planningGroups.Add(root); // Should set Id
		}

		public void Remove(PlanningGroup root)
		{
			((IDeleteTag)Get(root.Id.GetValueOrDefault())).SetDeleted();
		}

		public PlanningGroup Get(Guid id)
		{
			return _planningGroups.FirstOrDefault(x => x.Id == id);
		}

		public PlanningGroup Load(Guid id)
		{
			return _planningGroups.First(x => x.Id == id);
		}

		public IEnumerable<PlanningGroup> LoadAll()
		{
			return _planningGroups;
		}

		public FakePlanningGroupRepository Has(PlanningGroup root)
		{
			_planningGroups.Add(root);
			return this;
		}

		public PlanningGroup Has()
		{
			var planningGroup = new PlanningGroup().WithId();
			_planningGroups.Add(planningGroup);
			return planningGroup;
		}
	}
}