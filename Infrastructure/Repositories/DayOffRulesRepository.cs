﻿using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DayOffRulesRepository : Repository<PlanningGroupSettings>, IDayOffRulesRepository
	{
		public DayOffRulesRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
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

		public IList<PlanningGroupSettings> LoadAllByPlanningGroup(IPlanningGroup planningGroup)
		{
			return Session.CreateCriteria(typeof(PlanningGroupSettings), "dayOffRules")
				.Add(Restrictions.Eq("dayOffRules.PlanningGroup", planningGroup))
				 .SetResultTransformer(Transformers.DistinctRootEntity)
				 .List<PlanningGroupSettings>();
		}

		public IList<PlanningGroupSettings> LoadAllWithoutPlanningGroup()
		{
			if (Default() == null)
				lock (addLocker)
				{
					Add(PlanningGroupSettings.CreateDefault());
					Session.Flush();
				}
			return Session.CreateCriteria(typeof(PlanningGroupSettings), "dayOffRules")
				.Add(Restrictions.IsNull("dayOffRules.PlanningGroup"))
				 .SetResultTransformer(Transformers.DistinctRootEntity)
				 .List<PlanningGroupSettings>();
		}

		public void RemoveForPlanningGroup(IPlanningGroup planningGroup)
		{
			var dayOffRules = LoadAllByPlanningGroup(planningGroup);
			foreach (var dayOffRule in dayOffRules)
				base.Remove(dayOffRule);
		}
	}
}