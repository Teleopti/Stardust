using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[RtaTest]
	public class TeleoptiRtaServiceResolveTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule(new RtaAreaModule(configuration));
		}

		public TeleoptiRtaService TeleoptiRtaService;

		[Test]
		public void ShouldResolveTeleoptiRtaService()
		{
			TeleoptiRtaService.Should().Not.Be.Null();
		}

	}
}