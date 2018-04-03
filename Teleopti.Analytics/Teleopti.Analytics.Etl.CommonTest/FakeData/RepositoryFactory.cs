using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.CommonTest.Infrastructure;


namespace Teleopti.Analytics.Etl.CommonTest.FakeData
{
	static class RepositoryFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static IJobLogRepository GetLogRepository()
		{
			var rep = new RepositoryStub();

			rep.AddSchedule(1, 25, null);
			rep.AddSchedule(1, 35, null);
			rep.AddSchedule(1, 10, 0, 0, null);
			
			rep.AddLog(1, 10);
			rep.AddLog(2, 20);
			rep.AddLog(3, 30);

			return rep;
		}
	}
}