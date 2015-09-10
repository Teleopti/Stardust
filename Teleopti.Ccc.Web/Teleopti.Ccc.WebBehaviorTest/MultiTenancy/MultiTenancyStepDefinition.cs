using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.MultiTenancy
{
	[Binding]
	public class MultiTenancyStepDefinition
	{
		[Given(@"There is a tenant called '(.*)'")]
		public void GivenThereIsATenantCalled(string tenantName)
		{
			TenantUnitOfWorkState.TenantUnitOfWorkAction(tenantSession =>
			{
				var appDbConnstring = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().ConnectionString;
				var tenant = new Tenant(tenantName);
				tenant.DataSourceConfiguration.SetApplicationConnectionString(appDbConnstring);
				new PersistTenant(tenantSession).Persist(tenant);
			});
		}
	}
}