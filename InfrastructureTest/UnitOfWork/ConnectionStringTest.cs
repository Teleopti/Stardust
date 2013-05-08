using NUnit.Framework;
using SharpTestsEx;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class ConnectionStringTest
	{
		[Test]
		public void ApplicationConnectionString()
		{
			SetupFixtureForAssembly.DataSource.Application.ConnectionString.Length.Should().Be.GreaterThan(3);
		}

		[Test]
		public void MatrixConnectionString()
		{
			SetupFixtureForAssembly.DataSource.Statistic.ConnectionString.Length.Should().Be.GreaterThan(3);
		}

		[Test]
		public void ApplicationAndMatrixConnectionStringsShouldNotBeTheSame()
		{
			SetupFixtureForAssembly.DataSource.Application.ConnectionString
														 .Should().Not.Be.EqualTo(SetupFixtureForAssembly.DataSource.Statistic.ConnectionString);
		}
	}
}