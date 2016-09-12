using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class SqlServerTransientErrorTest
	{
		[Test]
		public void ShouldHandleServerNotAvailable()
		{
			var ex = SqlExceptionConstructor.CreateSqlException("asdf",40);
			new SqlTransientErrorDetectionStrategyWithTimeouts().IsTransient(ex).Should().Be.True();
		}
		
		[Test]
		public void ShouldHandleConnectionForciblyClosed()
		{
			var ex = SqlExceptionConstructor.CreateSqlException("An existing connection was forcibly closed by the remote host", 0);
			new SqlTransientErrorDetectionStrategyWithTimeouts().IsTransient(ex).Should().Be.True();
		}
		
		[Test]
		public void ShouldHandleConnectionRecoveryNotPossible()
		{
			var ex = SqlExceptionConstructor.CreateSqlException("The connection is broken and recovery is not possible. The connection is marked by the server as unrecoverable. No attempt was made to restore the connection.", 0);
			new SqlTransientErrorDetectionStrategyWithTimeouts().IsTransient(ex).Should().Be.True();
		}
	}
}