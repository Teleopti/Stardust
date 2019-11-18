using ManagerTest.Database;
using NUnit.Framework;

namespace ManagerTest
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
		public void BaseTestSetup()
		{
			_databaseHelper.TryClearDatabase();
		}
	}
}