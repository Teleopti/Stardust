using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface ILogonModel
	{
		IDataSourceContainer SelectedDataSourceContainer { get; set; }
		IList<string> Sdks { get; set; }
		string SelectedSdk { get; set; }
		IList<IBusinessUnit> AvailableBus { get; set; }
		IBusinessUnit SelectedBu { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		bool HasValidLogin();
		AuthenticationTypeOption AuthenticationType { get; set; }
		string Warning { get; set; }
		bool WindowsIsPossible { get; set; }
	}

	public class LogonModel : ILogonModel
	{
		public LogonModel()
		{
			AuthenticationType = AuthenticationTypeOption.Windows;
		}
		public IDataSourceContainer SelectedDataSourceContainer { get; set; }
		public IList<string> Sdks { get; set; }
		public string SelectedSdk { get; set; }
		public IList<IBusinessUnit> AvailableBus { get; set; }
		public IBusinessUnit SelectedBu { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool WindowsIsPossible { get; set; }
		public bool HasValidLogin()
		{
			return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
		}

		public AuthenticationTypeOption AuthenticationType { get; set; }
		public string Warning { get; set; }
	}
}