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

			rep.AddSchedule(1, 25);
			rep.AddSchedule(1, 35);
			rep.AddSchedule(1, 10, 0, 0);
			
			rep.AddLog(1, 10);
			rep.AddLog(2, 20);
			rep.AddLog(3, 30);

			return rep;
		}
	}
}