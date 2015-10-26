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

		public override void Add(DayOffSettings root)
		{
			if(root.Default)
				throw new ArgumentException("Cannot persist a new DayOffSetting set as default.");
			base.Add(root);
		}

		public override void Remove(DayOffSettings root)
		{
			if(root.Default)
				throw new ArgumentException("Cannot remove default DayOffSettings.");
			base.Remove(root);
		}
	}
}