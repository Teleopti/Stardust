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
		[Test]
		public void ShouldResolvePerformAllBadgeCalculationWhenToggleOn()
		{
			var builder = new ContainerBuilder();
			var toggleManager = new FakeToggleManager(Toggles.WFM_Gamification_Calculate_Badges_47250);
			builder.RegisterModule(CommonModule.ForTest(toggleManager));

			using (var container = builder.Build())
			{
				var result = container.Resolve<IPerformBadgeCalculation>();

				result.GetType().Should().Be.EqualTo(new PerformAllBadgeCalculation(null,null,null,null).GetType());
			}
		}

		[Test]
		public void ShouldResolvePerformBadgeCalculationWhenToggleOff()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());

			using (var container = builder.Build())
			{
				var result = container.Resolve<IPerformBadgeCalculation>();

				result.GetType().Should().Be.EqualTo(new PerformBadgeCalculation(null, null, null, null).GetType());
			}
		}

	}
}
