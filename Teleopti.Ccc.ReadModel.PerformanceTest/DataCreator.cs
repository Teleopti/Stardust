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

		[TestLogTime]
		public virtual void Create()
		{
			_database
				.WithDefaultScenario("Default")
				.WithActivity("Phone")
				.WithActivity("Lunch")
				.WithActivity("Break")
				;

			_configuration.NumberOfAgents.Times(() =>
			{
				_database.WithAgent();
			});
		}
	}
}