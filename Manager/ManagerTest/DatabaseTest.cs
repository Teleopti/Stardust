using ManagerTest.Database;
using NUnit.Framework;

namespace ManagerTest
{
	
	
	public class DatabaseTest
	{
		private DatabaseHelper _databaseHelper;

		[OneTimeSetUp]
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