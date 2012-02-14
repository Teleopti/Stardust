using Teleopti.Ccc.Sdk.Client;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class Session : ISessionStateProvider {
		public bool IsInitialized { get { return true; } }
		public bool IsLoggedIn { get { return LoggedOnPerson != null; } }

		public PersonDto LoggedOnPerson { get; private set; }
		public BusinessUnitDto BusinessUnit { get; private set; }
		public DataSourceDto DataSource { get; private set; }
		public string Password { get; private set; }

		public void SetSessionData(PersonDto loggedOnPerson, BusinessUnitDto businessUnit, DataSourceDto dataSource, string currentPassword)
		{
			LoggedOnPerson = loggedOnPerson;
			BusinessUnit = businessUnit;
			DataSource = dataSource;
			Password = currentPassword;
		}
	}
}