﻿namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Dto to hold layers representing an old Mainshift
	/// </summary>
	public interface IEditorShift : IShift
	{
		/// <summary>
		/// Returns the shift category
		/// </summary>
		IShiftCategory ShiftCategory { get; set; }
	}
}