using System;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DayOffSettingsRepository : Repository<DayOffSettings>, IDayOffSettingsRepository
	{
		public DayOffSettingsRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		private static readonly object addLocker = new object();
		public override void Add(DayOffSettings root)
		{
			if (root.Default && !root.Id.HasValue)
			{
				lock (addLocker)
				{
					var currentDefault = Default();
					if (currentDefault != null)
					{
						base.Remove(Default());
					}
				}
			}
			base.Add(root);
		}

		public override void Remove(DayOffSettings root)
		{
			if(root.Default)
				throw new ArgumentException("Cannot remove default DayOffSettings.");
			base.Remove(root);
		}

		public DayOffSettings Default()
		{
			return Session.CreateQuery("select dor from DayOffSettings dor where defaultsettings=1").UniqueResult<DayOffSettings>();
		}
	}
}