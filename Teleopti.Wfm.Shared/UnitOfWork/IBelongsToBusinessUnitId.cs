using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBelongsToBusinessUnitId
	{
		Guid? BusinessUnit { get; set; }
	}
}