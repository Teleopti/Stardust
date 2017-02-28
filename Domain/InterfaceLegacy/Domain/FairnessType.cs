using System;

namespace Teleopti.Interfaces.Domain
{
	[Flags]
	public enum FairnessType
	{
		EqualNumberOfShiftCategory = 1,
		Seniority = 2
	}
}