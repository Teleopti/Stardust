using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningGroupSettingsRepository : Repository<PlanningGroupSettings>, IPlanningGroupSettingsRepository
	{
		public PlanningGroupSettingsRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		private static readonly object addLocker = new object();

		public override void Add(PlanningGroupSettings root)
		{
			if (root.Default && !root.Id.HasValue && root.PlanningGroup == null)
			{
				lock (addLocker)
				{
					var currentDefault = Default();
					if (currentDefault != null)
					{
						base.Remove(currentDefault);
						Session.Flush();
					}
				}
			}
			base.Add(root);
		}

		public override void Remove(PlanningGroupSettings root)
		{
			if(root.Default)
				throw new ArgumentException("Cannot remove default PlanningGroupSettings.");
			base.Remove(root);
		}

		private PlanningGroupSettings Default()
		{
			return Session.GetNamedQuery("loadGlobalDefault").UniqueResult<PlanningGroupSettings>();
		}

		public AllPlanningGroupSettings LoadAllByPlanningGroup(IPlanningGroup planningGroup)
		{
			return new AllPlanningGroupSettings(Session.CreateCriteria(typeof(PlanningGroupSettings), "planningGroupSettings")
				.Add(Restrictions.Eq("planningGroupSettings.PlanningGroup", planningGroup))
				.AddOrder(Order.Asc("planningGroupSettings.Default"))
				.AddOrder(Order.Desc("planningGroupSettings.Priority"))
				.List<PlanningGroupSettings>());
		}

		public void RemoveForPlanningGroup(IPlanningGroup planningGroup)
		{
			var planningGroupSettingses = LoadAllByPlanningGroup(planningGroup);
			foreach (var planningGroupSettings in planningGroupSettingses)
				base.Remove(planningGroupSettings);
		}
	}
}