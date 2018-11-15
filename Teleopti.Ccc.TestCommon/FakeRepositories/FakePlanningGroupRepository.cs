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
			_planningGroups.Add(root);
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

		public PlanningGroup Has(PlanningGroup root)
		{
			_planningGroups.Add(root);
			return root;
		}

		public PlanningGroup Has()
		{
			return Has(new PlanningGroup().WithId());
		}

		public void HasDefault(PlanningGroup planningGroup, Action<PlanningGroupSettings> action)
		{
			var currDefault = planningGroup.Settings.SingleOrDefault(x => x.Default);
			if (currDefault == null)
			{
				currDefault = PlanningGroupSettings.CreateDefault();
				planningGroup.AddSetting(currDefault);
			}
			
			action(currDefault);
		}
	}
}