using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public interface ITransparentControlHelper
	{
		MinMax<int> MinMaxBorders { get; set; }
		MinMax<TimeSpan> MinMaxTime { get; set; }
		MinMax<int> MinMaxTimePos { get; set; }
		int SnapTo { get; set; }
		int GetLeftPosition(int leftPos, int rightPos);
		int GetRightPosition(int leftPos, int rightPos);
		int GetLeftPositionConsiderWidth(int leftPos, int width);
		int GetPositionFromTimeSpan(TimeSpan timeSpan);
		TimeSpan GetTimeSpanFromPosition(int pos);
		int GetSnappedPosition(int pos);
		bool IsRightToLeft { get; set; }
		int ScrollAdjust { get; set; }
		int MinWidth();
	}
}
