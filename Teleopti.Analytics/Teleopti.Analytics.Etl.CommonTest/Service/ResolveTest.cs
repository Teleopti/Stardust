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
	public class ResolveTest : IIsolateSystem, IExtendSystem
	{		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddModule(new EtlModule(configuration));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
		}
		
		public EtlService Service;

		[Test]
		public void ShouldResolve()
		{
			Service.Should().Not.Be.Null();
		}

	}
}
