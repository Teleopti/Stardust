using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleLoader : IScheduleLoader
	{
		private readonly IDatabaseReader _databaseReader;

		public ScheduleLoader(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public IEnumerable<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			return _databaseReader.GetCurrentSchedule(personId);
		}
	}
}