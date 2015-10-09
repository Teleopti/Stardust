using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
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
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new SchedulingCommonModule(configuration));
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