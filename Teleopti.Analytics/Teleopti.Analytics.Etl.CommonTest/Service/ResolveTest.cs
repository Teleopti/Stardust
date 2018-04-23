using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
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
			
			// same as domain test attribute does, sweet!
			var tenants = new FakeAllTenantEtlSettings();
			tenants.Has(DomainTestAttribute.DefaultTenantName);
			system.UseTestDouble(tenants).For<IAllTenantEtlSettings>();
		}
		
		public EtlService Service;

		[Test]
		public void ShouldResolve()
		{
			Service.Should().Not.Be.Null();
		}

	}
}
