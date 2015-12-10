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
	}
}