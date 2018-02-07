using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;

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
			var toggleManager = new FakeToggleManager(Toggles.WFM_Gamification_Calculate_Badges_47250);
			builder.RegisterModule(CommonModule.ForTest(toggleManager));

			using (var container = builder.Build())
			{
				var result = container.Resolve<IPerformBadgeCalculation>();
				result.Should().Be.InstanceOf<PerformAllBadgeCalculation>();
			}
		}

		[Test]
		public void ShouldResolvePerformBadgeCalculationWhenToggleOff()
		{
			using (var container = _builder.Build())
			{
				var result = container.Resolve<IPerformBadgeCalculation>();
				result.Should().Be.InstanceOf<PerformBadgeCalculation>();
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
