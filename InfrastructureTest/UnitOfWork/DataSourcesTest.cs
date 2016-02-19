using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[Category("LongRunning")]
	public class DataSourcesTest
	{
		private MockRepository mocks;
		private IUnitOfWorkFactory application;
		private IUnitOfWorkFactory statistics;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			application = mocks.StrictMock<IUnitOfWorkFactory>();
			statistics = mocks.StrictMock<IUnitOfWorkFactory>();
		}

		[Test]
		public void VerifyProperties()
		{
			DataSource target = new DataSource(application, statistics, null);
			Assert.AreSame(application, target.Application);
			Assert.AreSame(statistics, target.Analytics);
		}

		[Test]
		public void VerifyCanResetDataSource()
		{
			DataSource target = new DataSource(application, statistics, null);
			target.ResetStatistic();
			Assert.IsNull(target.Analytics);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ApplicationMustNotBeNull()
		{
			new DataSource(null, statistics, null);
		}

		[Test]
		public void ShouldDisposeBothSessionFactories()
		{
			var target = new DataSource(application, statistics, null);
			using (mocks.Record())
			{
				application.Dispose();
				statistics.Dispose();
			}
			using (mocks.Playback())
			{
				target.Dispose();
			}
		}

		[Test]
		public void ShouldDisposeApplicationSessionFactory()
		{
			var target = new DataSource(application, null, null);
			using (mocks.Record())
			{
				application.Dispose();
			}
			using (mocks.Playback())
			{
				target.Dispose();
			}
		}

		[Test]
		public void DataSourceNameShouldBeSameAsApplicationName()
		{
			var target = new DataSource(application, statistics, null);
			var name = Guid.NewGuid().ToString();
			using (mocks.Record())
			{
				Expect.Call(application.Name).Return(name);
			}
			using (mocks.Playback())
			{
				target.DataSourceName
					.Should().Be.EqualTo(name);
			}
		}
	}
}
