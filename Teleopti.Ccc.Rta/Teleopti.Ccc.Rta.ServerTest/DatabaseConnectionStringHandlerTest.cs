using NUnit.Framework;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public class DatabaseConnectionStringHandlerTest
	{
		private IDatabaseConnectionStringHandler _target;

		[SetUp]
		public void Setup()
		{
			_target = new DatabaseConnectionStringHandler();
		}

		[Test]
		public void ShouldReturnAppConnectionString()
		{
			var result = _target.AppConnectionString();
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ShouldReturnDataStoreConnectionString()
		{
			var result = _target.DataStoreConnectionString();
			Assert.That(result, Is.Null);
		}
	}
}
