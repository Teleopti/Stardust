using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
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
