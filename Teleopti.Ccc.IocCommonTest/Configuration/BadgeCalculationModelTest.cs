using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class BadgeCalculationModelTest
	{
		private ContainerBuilder _builder;

		[SetUp]
		public void Setup()
		{
			_builder = new ContainerBuilder();
			_builder.RegisterModule(CommonModule.ForTest());
		}

		[Test]
		public void ShouldResolvePerformAllBadgeCalculationWhenToggleOn()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());

			using (var container = builder.Build())
			{
				var result = container.Resolve<IPerformBadgeCalculation>();
				result.Should().Be.InstanceOf<PerformAllBadgeCalculation>();
			}
		}

		[Test]
		public void ShouldResolvePushMessageSender()
		{
			using (var container = _builder.Build())
			{
				container.Resolve<IPushMessageSender>().Should().Be.InstanceOf<PushMessageSender>();
			}
		}

	}
}
