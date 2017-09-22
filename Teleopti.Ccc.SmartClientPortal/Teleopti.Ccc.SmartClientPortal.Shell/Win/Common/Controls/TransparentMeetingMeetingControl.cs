using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
	public partial class TransparentMeetingMeetingControl : UserControl, ITransparentMeetingControlView
	{
		private TransparentMeetingControlPresenter _presenter;
		
		public TransparentMeetingMeetingControl()
		{
			InitializeComponent();
		}

		public void RefreshControl()
		{
			panelWest.Left = ClientRectangle.Left;
			panelEast.Left = ClientRectangle.Right - panelEast.Width;

			panelWest.BringToFront();
			panelEast.BringToFront();
		}

		public void InitControl(TransparentMeetingControlModel transparentMeetingControlModel, ITransparentControlHelper transparentControlHelper)
		{
			if(transparentMeetingControlModel == null)
				throw new ArgumentNullException("transparentMeetingControlModel");
		
			_presenter = new TransparentMeetingControlPresenter(this, transparentMeetingControlModel, transparentControlHelper);
			_presenter.Initialize();
			Parent = (Control)transparentMeetingControlModel.Parent;
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if(e == null)
				throw new ArgumentNullException("e");

			if (_presenter.UpdateBackground)
			{
				using (Brush brush = new SolidBrush(Color.FromArgb(_presenter.Transparency, _presenter.BackColor)))
				{
					using (var region = new Region(e.ClipRectangle))
					{
						e.Graphics.FillRegion(brush, region);
					}
				}
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ExStyle |= 0x20;
				return cp;
			}
		}

		private void PanelWestMouseDown(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseDown(e.X);
		}

		private void PanelWestMouseUp(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseUp(true, false);
		}

		private void PanelWestMouseMove(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseMoveBorderWest(e.X);
		}

		private void PanelEastMouseDown(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseDown(e.X);
		}

		private void PanelEastMouseUp(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseUp(false, true);
		}

		private void PanelEastMouseMove(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseMoveBorderEast(e.X);
		}

		private void TransparentControlMouseDown(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseDown(e.X);
		}

		private void TransparentControlMouseUp(object sender, MouseEventArgs e)
		{
			_presenter.OnMouseUp(false, false);
		}

		private void TransparentControlMouseMove(object sender, MouseEventArgs e)
		{
			_presenter.OnMoveMouse(e.X);
		}

		public void InvalidateParent()
		{
			if (Parent == null)
				return;

			var rc = new Rectangle(Left, Top, Width, Height);
			Parent.Invalidate(rc, true);
		}

		public void Position(TransparentMeetingControlModel meetingControlModel)
		{
			if(meetingControlModel == null)
				throw new ArgumentNullException("meetingControlModel");

			Left = meetingControlModel.Left;
			Top = meetingControlModel.Top;
			Height = meetingControlModel.Height;
			Width = meetingControlModel.Width;
		}

		public void SetBorderWidth(int width)
		{
			panelWest.Width = width;
			panelEast.Width = width;
		}

		public void SetWestBorderColor(Color color)
		{
			panelWest.BackColor = color;
		}

		public void SetEastBorderColor(Color color)
		{
			panelEast.BackColor = color;
		}	
	}
}
