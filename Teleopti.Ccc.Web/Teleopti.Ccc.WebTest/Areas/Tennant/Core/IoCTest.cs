using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class IoCTest
	{
		[Test]
		public void CanResolveApplicationAuthentication()
		{
			using (var container = buildContainer())
			{
				container.Resolve<IApplicationAuthentication>()
					.Should().Not.Be.Null();
			}
		}

		private static IContainer buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<TennantModule>();
			//temp - remove later -> this will be a seperate app
			builder.RegisterModule(CommonModule.ForTest());

			return builder.Build();
		}
	}
}