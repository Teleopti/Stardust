using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
	public class PersonContainer
	{
		public PersonContainer(IPerson person, string userName, string password, string dataSource, string tenantPassword)
		{
			Person = person;
			UserName = userName;
			Password = password;
			DataSource = dataSource;
			TenantPassword = tenantPassword;
		}

		public IPerson Person { get; private set; }
		public string UserName { get; private set; }
		public string Password { get; private set; }
		public string DataSource { get; private set; }
		public string TenantPassword { get; set; }
	}
}