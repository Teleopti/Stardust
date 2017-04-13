using System.Drawing;

namespace Teleopti.Ccc.WinCode.Common
{
	public interface ITransparentMeetingControlView
	{
		//void Configure(object parent, Color color);
		void Position(TransparentMeetingControlModel meetingControlModel);
		void InvalidateParent();
		//void PaintBackground(Brush brush, Graphics graphics, Region region);
		void SetBorderWidth(int width);
		void SetWestBorderColor(Color color);
		void SetEastBorderColor(Color color);
	}
}
