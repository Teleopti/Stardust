using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface IGetBusinessUnitId
	{
		Guid Get(Guid teamId);
	}
}