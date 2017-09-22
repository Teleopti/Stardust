using System.Data.SqlClient;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class TeleoptiLatencySqlDriverTest
	{
		private TeleoptiLatencySqlDriver target;

		[SetUp]
		public void Setup()
		{
			target = new TeleoptiLatencySqlDriver();
		}

		[Test]
		public void ShouldReturnASqlCommand()
		{
			target.CreateCommand().Should().Be.InstanceOf<SqlCommand>();
		}

		[Test]
		public void ShouldReturnNewInstance()
		{
			target.CreateCommand()
				.Should().Not.Be.SameInstanceAs(target.CreateCommand());
		}

		[Test]
		public void ShouldFetchConfigLatencyValue()
		{
			const int latency = 37;
			
			var configReader = MockRepository.GenerateMock<IConfigReader>();
			target.ConfigReader = configReader;

			configReader.Expect(cr => cr.AppConfig("latency")).Return(latency.ToString());

			target.Latency.Should().Be.EqualTo(latency);
		}

		[Test]
		public void ShouldOnlyCallConfigReaderOnce()
		{
			var configReader = MockRepository.GenerateStrictMock<IConfigReader>();
			target.ConfigReader = configReader;

			configReader.Expect(cr => cr.AppConfig("latency")).Return(37.ToString());

			target.CreateCommand();
			target.CreateCommand();
			target.CreateCommand();
		}
	}
}