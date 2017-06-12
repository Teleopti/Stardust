using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DayOffRulesRepository : Repository<DayOffRules>, IDayOffRulesRepository
	{
		public DayOffRulesRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		private static readonly object addLocker = new object();

		public override void Add(DayOffRules root)
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

		public override void Remove(DayOffRules root)
		{
			if(root.Default)
				throw new ArgumentException("Cannot remove default DayOffRules.");
			base.Remove(root);
		}

		private DayOffRules Default()
		{
			return Session.GetNamedQuery("loadGlobalDefault").UniqueResult<DayOffRules>();
		}

		public IList<DayOffRules> LoadAllByPlanningGroup(IPlanningGroup planningGroup)
		{
			return Session.CreateCriteria(typeof(DayOffRules), "dayOffRules")
				.Add(Restrictions.Eq("dayOffRules.PlanningGroup", planningGroup))
				 .SetResultTransformer(Transformers.DistinctRootEntity)
				 .List<DayOffRules>();
		}

		public IList<DayOffRules> LoadAllWithoutPlanningGroup()
		{
			if (Default() == null)
				lock (addLocker)
				{
					Add(DayOffRules.CreateDefault());
					Session.Flush();
				}
			return Session.CreateCriteria(typeof(DayOffRules), "dayOffRules")
				.Add(Restrictions.IsNull("dayOffRules.PlanningGroup"))
				 .SetResultTransformer(Transformers.DistinctRootEntity)
				 .List<DayOffRules>();
		}

		public void RemoveForPlanningGroup(IPlanningGroup planningGroup)
		{
			var dayOffRules = LoadAllByPlanningGroup(planningGroup);
			foreach (var dayOffRule in dayOffRules)
				base.Remove(dayOffRule);
		}
	}
}