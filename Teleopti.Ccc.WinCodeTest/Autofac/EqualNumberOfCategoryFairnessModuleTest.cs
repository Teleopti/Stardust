using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.WinCode.Autofac;

namespace Teleopti.Ccc.WinCodeTest.Autofac
{
	[TestFixture]
	public class EqualNumberOfCategoryFairnessModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule<CommonModule>();
			_containerBuilder.RegisterModule<SchedulingCommonModule>();

		}

		[Test]
		public void ShouldResolve()
		{
			_containerBuilder.RegisterModule<EqualNumberOfCategoryFairnessModule>();
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IEqualNumberOfCategoryFairnessService>()
								 .Should().Not.Be.Null();
			}
		}
	}
}