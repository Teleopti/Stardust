using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface ILogonModel
	{
		bool GetConfigFromWebService { get; set; }
		IList<IDataSourceContainer> DataSourceContainers { get; set; }
		IDataSourceContainer SelectedDataSourceContainer { get; set; }
		IList<string> Sdks { get; set; }
		string SelectedSdk { get; set; }
		IList<IBusinessUnit> AvailableBus { get; set; }
		IBusinessUnit SelectedBu { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		bool HasValidLogin();
	}
}