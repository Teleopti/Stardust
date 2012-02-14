using System;
using System.Drawing;

namespace Teleopti.Ccc.WinCode.Common
{
	public class TransparentMeetingControlModel
	{
		public event EventHandler<EventArgs> TransparentControlModelChanged;

		public TransparentMeetingControlModel(int left, int width, Object parent, Color backColor, byte transparency)
		{
			Left = left;
			Width = width;
			Parent = parent;
			BackColor = backColor;
			Transparency = transparency;
			BorderWidth = 3;
			LeftBorderColor = Color.Blue;
			RightBorderColor = Color.Blue;
		}

		public int Top { get; set; }
		public int Height { get; set; }
		public int Width { get; set; }
		public int Left { get; set; }
		public Color BackColor { get; private set; }
		public byte Transparency { get; private set; }
		public object Parent { get; set; }
		public int BorderWidth { get; set; }
		public Color LeftBorderColor { get; set; }
		public Color RightBorderColor { get; set; }
		public bool ModelChanged { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		public void RaisePossibleChangeEvent()
		{
			if (ModelChanged)
			{
				if (TransparentControlModelChanged != null)
				{
					ModelChanged = false;
					TransparentControlModelChanged(this, EventArgs.Empty);
				}
			}
		}
	}
}
