using System;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class TransparentControlMeetingHelper : ITransparentControlHelper
	{
		public MinMax<int> MinMaxBorders { get; set; }
		public MinMax<TimeSpan> MinMaxTime { get; set; }
		public MinMax<int> MinMaxTimePos { get; set; }
		public int SnapTo { get; set; }
		public bool IsRightToLeft { get; set; }
		public int ScrollAdjust { get; set; }

		public TransparentControlMeetingHelper(MinMax<int> minMaxBorders, MinMax<TimeSpan> minMaxTime, MinMax<int> minMaxTimePos, int snapToMinutes)
		{
			MinMaxBorders = minMaxBorders;
			MinMaxTime = minMaxTime;
			MinMaxTimePos = minMaxTimePos;
			SnapTo = snapToMinutes;
		}

		public int MinWidth()
		{
			var diffMinMaxMinutes = MinMaxTime.Maximum.Subtract(MinMaxTime.Minimum).TotalMinutes;
			var diffMinMaxPixels = MinMaxTimePos.Maximum - MinMaxTimePos.Minimum;
			return (int)((diffMinMaxPixels / diffMinMaxMinutes) * SnapTo);	
		}

		public int GetLeftPosition(int leftPos, int rightPos)
		{
			if (leftPos < MinMaxBorders.Minimum)
				return MinMaxBorders.Minimum;

			if (leftPos > rightPos)
				return rightPos - MinWidth();
				
			return leftPos;
		}
		
		public int GetRightPosition(int leftPos, int rightPos)
		{
			if (rightPos > MinMaxBorders.Maximum)
				return MinMaxBorders.Maximum;

			if (rightPos < leftPos)
				return leftPos + MinWidth();

			return rightPos;
		}

		public int GetLeftPositionConsiderWidth(int leftPos, int width)
		{
			if (leftPos < MinMaxBorders.Minimum)
				return MinMaxBorders.Minimum;

			if ((leftPos + width) > MinMaxBorders.Maximum)
				return MinMaxBorders.Maximum - width;

			return leftPos;
	
		}

		public int GetColumnFromTimeSpan(TimeSpan timeSpan, int headerWidth, int resolution)
		{
			var diffMinMaxMinutes = MinMaxTime.Maximum.Subtract(MinMaxTime.Minimum).TotalMinutes;
			var diffMinMaxPixels = MinMaxTimePos.Maximum - MinMaxTimePos.Minimum;
			var pixelsPerMinute = diffMinMaxPixels / diffMinMaxMinutes;
			var pos = GetPositionFromTimeSpan(timeSpan);

			if (IsRightToLeft)
				pos = MinMaxBorders.Maximum - pos + MinMaxTimePos.Minimum;

			return (int)((pos - headerWidth) / pixelsPerMinute / resolution);
		}

		public int GetPositionFromTimeSpan(TimeSpan timeSpan)
		{
			//if (timeSpan < MinMaxTime.Minimum || timeSpan > MinMaxTime.Maximum)
			//    throw new ArgumentOutOfRangeException("timeSpan");

			var diffMinutes = timeSpan.Subtract(MinMaxTime.Minimum).TotalMinutes;
			var diffMinMaxMinutes = MinMaxTime.Maximum.Subtract(MinMaxTime.Minimum).TotalMinutes;
			var diffMinMaxPixels = MinMaxTimePos.Maximum - MinMaxTimePos.Minimum;
			var pixelsPerMinute = diffMinMaxPixels / diffMinMaxMinutes;
			
			if (IsRightToLeft)
				return (int)Math.Ceiling(MinMaxBorders.Maximum - (diffMinutes * pixelsPerMinute)) - ScrollAdjust; //TODO FIX + Scrolladjust?

			return (int)(diffMinutes * pixelsPerMinute + MinMaxTimePos.Minimum - ScrollAdjust);
		}

		public TimeSpan GetTimeSpanFromPosition(int pos)
		{
			if (pos < MinMaxTimePos.Minimum || pos > MinMaxTimePos.Maximum)
				throw new ArgumentOutOfRangeException("pos");

			var diffPixels = pos - MinMaxTimePos.Minimum;

			if (IsRightToLeft)
				diffPixels = MinMaxBorders.Maximum - pos;
			
			var diffMinMaxMinutes = MinMaxTime.Maximum.Subtract(MinMaxTime.Minimum).TotalMinutes;
			var diffMinMaxPixels = MinMaxTimePos.Maximum - MinMaxTimePos.Minimum;
			var pixelsPerMinute = diffMinMaxPixels / diffMinMaxMinutes;
			var minutes = Math.Ceiling(diffPixels / pixelsPerMinute);
			return GetSnappedTime(TimeSpan.FromMinutes(minutes).Add(MinMaxTime.Minimum));
		}

		private TimeSpan GetSnappedTime(TimeSpan timeSpan)
		{
			var remainder = timeSpan.TotalMinutes % SnapTo;

			if (remainder == 0)
				return timeSpan;

			if (remainder < (Double)SnapTo / 2)
				return timeSpan.Subtract(TimeSpan.FromMinutes(remainder));

			return timeSpan.Add(TimeSpan.FromMinutes(SnapTo - remainder));
		}

		public int GetSnappedPosition(int pos)
		{
			if (pos < MinMaxTimePos.Minimum || pos > MinMaxTimePos.Maximum)
				return pos;

			var timeSpan = GetInternalTimeSpanFromPosition(pos);
			var remainder = timeSpan.TotalMinutes%SnapTo;

			if (remainder == 0)
				return pos;

			if(remainder < (Double)SnapTo / 2)
			{
				timeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(remainder));
				return GetPositionFromTimeSpan(timeSpan);
			}

			timeSpan = timeSpan.Add(TimeSpan.FromMinutes(SnapTo - remainder));
			return GetPositionFromTimeSpan(timeSpan);
		}

		private TimeSpan GetInternalTimeSpanFromPosition(int pos)
		{
			if (pos < MinMaxTimePos.Minimum || pos > MinMaxTimePos.Maximum)
				throw new ArgumentOutOfRangeException("pos");

			var diffPixels = pos - MinMaxTimePos.Minimum + ScrollAdjust; 

			if (IsRightToLeft)
				diffPixels = MinMaxBorders.Maximum - pos - ScrollAdjust; //TODO FIX - ScrollAdjust?

			var diffMinMaxMinutes = MinMaxTime.Maximum.Subtract(MinMaxTime.Minimum).TotalMinutes;
			var diffMinMaxPixels = MinMaxTimePos.Maximum - MinMaxTimePos.Minimum;
			var pixelsPerMinute = diffMinMaxPixels / diffMinMaxMinutes;

			var minutes = Math.Ceiling(diffPixels / pixelsPerMinute);

			if (IsRightToLeft)
				minutes = (int)(diffPixels/pixelsPerMinute);

			return TimeSpan.FromMinutes(minutes).Add(MinMaxTime.Minimum);
		}
	}
}
