using NUnit.Framework;

namespace ManagerTest.Database
{
	public class DatabaseTest
	{
		private DatabaseHelper _databaseHelper;

		[TestFixtureSetUp]
		public void BaseTestTestFixtureSetup()
		{
			_databaseHelper = new DatabaseHelper();
			_databaseHelper.Create();
		}

		[SetUp]
		public void BasteTestSetup()
		{
			_databaseHelper.TryClearDatabase();
		}
	}
}