using System;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

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
			if (root.Default && !root.Id.HasValue)
			{
				lock (addLocker)
				{
					var currentDefault = Default();
					if (currentDefault != null)
					{
						base.Remove(Default());
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

		public DayOffRules Default()
		{
			//TODO: this should be handled differently when views/workflow is more stable -> don't create implicitly here
			//Also, when done, remove similar logic in repofake
			var defaultSettingInDb =  Session.GetNamedQuery("loadDefault").UniqueResult<DayOffRules>();
			return defaultSettingInDb ?? DayOffRules.CreateDefault();
		}
	}
}