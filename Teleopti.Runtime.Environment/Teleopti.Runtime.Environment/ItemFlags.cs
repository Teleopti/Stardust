using System;

namespace Teleopti.Runtime.Environment
{
	[Flags]
	public enum ItemFlags
	{
		mfUnchecked = 0x00000000,
		mfString = 0x00000000,
		mfChecked = 0x00000008,
		mfByPosition = 0x00000400,
		mfSeparator = 0x00000800
	}
}