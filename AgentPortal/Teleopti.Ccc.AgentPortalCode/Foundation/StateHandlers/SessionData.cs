using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
	public class SessionData : ISessionData
	{
		private readonly PersonDto _loggedPerson;
		private readonly BusinessUnitDto _businessUnit;
		private readonly DataSourceDto _dataSource;
		private IDictionary<string, string> _appSettings = new Dictionary<string, string>();

		public SessionData(PersonDto personDto, BusinessUnitDto businessUnit, DataSourceDto dataSourceDto, string password)
		{
			_loggedPerson = personDto;
			_businessUnit = businessUnit;
			_dataSource = dataSourceDto;
			Password = password;
		}

		public bool IsLoggedIn { get { return StateHolderReader.Instance.StateReader.IsLoggedIn; } }
		public PersonDto LoggedOnPerson { get { return _loggedPerson; } }
		public BusinessUnitDto BusinessUnit { get { return _businessUnit; } }
		public DataSourceDto DataSource { get { return _dataSource; } }
		public string Password { get; set; }

		public IDictionary<string, string> AppSettings { get { return _appSettings; } }

		public void AssignAppSettings(IDictionary<string, string> appSettings)
		{
			_appSettings = appSettings;
		}

		public void SetPassword(string password)
		{
			Password = password;
		}
	}
}
