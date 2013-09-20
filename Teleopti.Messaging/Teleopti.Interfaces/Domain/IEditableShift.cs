using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Dto to hold layers representing an old Mainshift
	/// </summary>
	public interface IEditableShift : IProjectionSource,
																ICloneableEntity<IEditableShift>
	{
		/// <summary>
		/// Returns the shift category
		/// </summary>
		IShiftCategory ShiftCategory { get; set; }

		IList<IEditableShiftLayer> LayerCollection {get; }
	}
}