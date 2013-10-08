using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Main
{
	public class LogonModel
	{
		public bool GetConfigFromWebService { get; set; }
		public IList<IDataSourceContainer> DataSourceContainers { get; set; }
        public IDataSourceContainer SelectedDataSourceContainer { get; set; }
        public IList<string> Sdks { get; set; }
        public string SelectedSdk { get; set; }
		public IList<IBusinessUnit> AvailableBus { get; set; }
        public IBusinessUnit SelectedBu { get; set; }
	    public string UserName { get; set; }
	    public string Password { get; set; }

        public bool HasValidLogin()
        {
            return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
        }
	}
}
