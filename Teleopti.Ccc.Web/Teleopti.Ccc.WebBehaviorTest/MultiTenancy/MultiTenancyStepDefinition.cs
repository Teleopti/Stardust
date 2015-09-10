using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
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
				var tenant = new Tenant(tenantName);
				new PersistTenant(tenantSession).Persist(tenant);
			});
		}
	}
}