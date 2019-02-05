using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IFilterOnBusinessUnitId
	{
		Guid? BusinessUnit { get; set; }
	}
}