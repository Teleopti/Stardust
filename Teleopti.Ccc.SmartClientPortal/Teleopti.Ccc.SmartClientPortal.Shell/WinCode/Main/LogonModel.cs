using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Main
{
	public class LogonModel
	{
		public LogonModel()
		{
			AuthenticationType = AuthenticationTypeOption.Windows;
		}
		public IDataSourceContainer SelectedDataSourceContainer { get; set; }
		public IList<IBusinessUnit> AvailableBus { get; set; }
		public IBusinessUnit SelectedBu { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool HasValidLogin()
		{
			return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
		}

		public AuthenticationTypeOption AuthenticationType { get; set; }
		public string Warning { get; set; }
		public Guid PersonId { get; set; }
	}
}