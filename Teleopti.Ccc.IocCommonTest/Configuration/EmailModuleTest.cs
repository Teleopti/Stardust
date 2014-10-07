using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EmailModuleTest
	{
		[Test]
		public void CanCreateEmailSender()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<EmailModule>();

			using (var ioc = builder.Build())
			{
				ioc.Resolve<IEmailSender>().Should().Not.Be.Null();
			}
		}
	}
}
