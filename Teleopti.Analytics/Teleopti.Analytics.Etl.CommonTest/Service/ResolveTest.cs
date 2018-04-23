using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	[DomainTest]
	public class ResolveTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new EtlModule(configuration));
			system.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
		}
		
		public EtlService Service;

		[Test]
		public void ShouldResolve()
		{
			Service.Should().Not.Be.Null();
		}

	}
}
