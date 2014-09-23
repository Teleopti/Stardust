using System;

namespace Teleopti.Interfaces.Domain
{
	[Flags]
	public enum FairnessType
	{
		FairnessPoints = 0,
		EqualNumberOfShiftCategory = 1,
		Seniority = 2
	}
}