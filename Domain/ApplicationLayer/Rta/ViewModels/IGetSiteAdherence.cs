using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface IGetSiteAdherence
	{
		IEnumerable<SiteOutOfAdherence> OutOfAdherence();
	}
}