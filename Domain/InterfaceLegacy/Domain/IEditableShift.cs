using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IEditableShift : IProjectionSource
	{
		IShiftCategory ShiftCategory { get; set; }
		List<IEditableShiftLayer> LayerCollection {get; }
		IEditableShift MakeCopy();
		IEditableShift MoveTo(DateOnly currentDate, DateOnly destinationDate);
	}
}