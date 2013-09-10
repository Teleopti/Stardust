using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Client;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
	public class SessionStateProvider : ISessionStateProvider
	{
		public bool IsLoggedIn
		{
			get
			{
				{
					if (StateHolderReader.IsInitialized)
					{
						return StateHolder.Instance.StateReader.SessionScopeData.IsLoggedIn;
					}
					return false;
				}
			}
		}

		public PersonDto LoggedOnPerson { get { return StateHolderReader.Instance.StateReader.SessionScopeData.LoggedOnPerson; } }
		public BusinessUnitDto BusinessUnit { get { return StateHolderReader.Instance.StateReader.SessionScopeData.BusinessUnit; } }
		public DataSourceDto DataSource { get { return StateHolderReader.Instance.StateReader.SessionScopeData.DataSource; } }
		public string Password { get { return StateHolderReader.Instance.StateReader.SessionScopeData.Password; } }
	}
}