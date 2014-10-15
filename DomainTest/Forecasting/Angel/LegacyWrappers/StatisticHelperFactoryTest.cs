using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.LegacyWrappers
{
	public class StatisticHelperFactoryTest
	{
		[Test]
		public void ShouldCreateInstance()
		{
			var target = new StatisticHelperFactory(new DummyCurrentUnitOfWork(), new RepositoryFactory());
			target.Create().Should().Be.OfType<StatisticHelper>();
		}
	}
}