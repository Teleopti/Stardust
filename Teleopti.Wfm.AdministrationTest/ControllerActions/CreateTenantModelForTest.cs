using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	public class CreateTenantModelForTest : CreateTenantModel
	{
		public CreateTenantModelForTest()
		{
			Tenant = "New Tenant";
			CreateDbUser = "dbcreatorperson";
			CreateDbPassword = "password";
			AppUser = "new TenantAppUser";
			AppPassword = "NewTenantAppPassword";
			FirstUser = "Thefirstone";
			FirstUserPassword = "Agood@pasw0rd";
			BusinessUnit = "My First BU";
		}
	}
}