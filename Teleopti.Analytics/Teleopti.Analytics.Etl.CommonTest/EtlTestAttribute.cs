using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Analytics.Etl.CommonTest
{
	public class EtlTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddModule(new EtlModule(configuration));
			system.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();

			// same as domain test attribute does, sweet!
			var tenants = new FakeAllTenantEtlSettings();
			tenants.Has(DefaultTenantName);
			system.UseTestDouble(tenants).For<IAllTenantEtlSettings>();
		}
	}
}