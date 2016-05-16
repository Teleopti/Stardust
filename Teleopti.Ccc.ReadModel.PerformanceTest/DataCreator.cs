using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	public class DataCreator
	{
		private readonly TestConfiguration _configuration;
		private readonly Database _database;

		public DataCreator(TestConfiguration configuration, Database database)
		{
			_configuration = configuration;
			_database = database;
		}

		[LogTime]
		[UnitOfWork]
		public virtual void Create()
		{
			_database.WithActivity("Phone");
			_database.WithActivity("Lunch");
			_database.WithActivity("Break");

			_configuration.NumberOfAgents.Times(() =>
			{
				_database.WithAgent();
			});
		}
	}
}