using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public interface ISiteView
	{
		void LoadSiteGrid(IList<ISite> allNotDeletedSites);
	}
}