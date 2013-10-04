using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Main
{
	public class LogonModel
	{
		public IList<IDataSourceContainer> DataSourceContainers { get; set; }
        public IDataSourceContainer SelectedDataSourceContainer { get; set; }
        public IList<string> Sdks { get; set; }
        public string SelectedSdk { get; set; }
        public string SelectedBu { get; set; }
	    public string UserName { get; set; }
	    public string Password { get; set; }

	    public List<IBusinessUnit> AvailableBus { get; set; }
	}
}
