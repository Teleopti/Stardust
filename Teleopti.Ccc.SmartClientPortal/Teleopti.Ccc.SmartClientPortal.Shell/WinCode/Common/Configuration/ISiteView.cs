using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public interface ISiteView
	{
		void LoadSiteGrid(IList<ISite> allNotDeletedSites);
	}
}