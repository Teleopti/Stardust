using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IEditableShift : IProjectionSource
	{
		IShiftCategory ShiftCategory { get; set; }
		IList<IEditableShiftLayer> LayerCollection {get; }
		IEditableShift MakeCopy();
		IEditableShift MoveTo(DateOnly currentDate, DateOnly destinationDate);
	}
}