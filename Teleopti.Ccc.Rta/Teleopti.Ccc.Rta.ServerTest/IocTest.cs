using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	public class IocTest
	{
		[Test]
		public void ShouldResolveRtaDataHandler()
		{
			using (var container = BuildContainer.Build())
			{
				container.Resolve<IRtaDataHandler>()
					.Should().Not.Be.Null();
			}
		}
	}
}
