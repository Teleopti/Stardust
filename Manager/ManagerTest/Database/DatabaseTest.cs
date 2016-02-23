using NUnit.Framework;

namespace ManagerTest.Database
{
	public class DatabaseTest
	{
		[SetUp]
		public void BaseTestSetup()
		{
			var databaseHelper = new DatabaseHelper();
			databaseHelper.Create();
		}
	}
}