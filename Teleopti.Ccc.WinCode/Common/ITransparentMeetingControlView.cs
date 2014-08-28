using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Controls;

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
