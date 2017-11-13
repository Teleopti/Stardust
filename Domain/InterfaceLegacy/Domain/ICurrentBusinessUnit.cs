using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ICurrentBusinessUnit
	{
		IBusinessUnit Current();
		Guid? CurrentId();
	}
}