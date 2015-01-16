using System.Data;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class TennantDatabaseConnectionStringTest
	{
		[Test]
		public void ShouldCreateConnection()
		{
			var target = new TennantDatabaseConnectionFactory(UnitOfWorkFactory.Current.ConnectionString);
			using (var conn = target.CreateConnection())
			{
				conn.ConnectionString.Should().Be.EqualTo(UnitOfWorkFactory.Current.ConnectionString);
				conn.State.Should().Be.EqualTo(ConnectionState.Open);
			}
		}
	}
}