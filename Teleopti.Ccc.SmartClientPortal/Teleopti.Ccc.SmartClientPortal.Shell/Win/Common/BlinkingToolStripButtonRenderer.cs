using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	public class BlinkingToolStripButtonRenderer : ToolStripProfessionalRenderer
	{
		private List<ToolStripItem> _blinkButtons = new List<ToolStripItem>();
		private bool _blink;
		private readonly Timer _blinkTimer;
		private readonly ToolStrip _strip;
		public BlinkingToolStripButtonRenderer(ToolStrip strip)
		{
			_strip = strip;
			_strip.Renderer = this;
			_strip.Disposed += stripDisposed;
			_blinkTimer = new Timer { Interval = 500 };
			_blinkTimer.Tick += delegate { _blink = !_blink; strip.Invalidate(); };
		}

		public void BlinkButton(ToolStripButton button, bool enable)
		{
			if (!enable)
			{
				_blinkButtons.Remove(button);
			}
			else
			{
				_blinkButtons.Add(button);
			}

			_blinkTimer.Enabled = _blinkButtons.Count > 0;
			_strip.Invalidate();
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (_blink && e.Item is ToolStripButton btn && _blinkButtons.Contains(btn))
			{
				Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
				e.Graphics.FillRectangle(Brushes.Orange, bounds);
			}
			else base.OnRenderButtonBackground(e);
		}

		private void stripDisposed(object sender, EventArgs e)
		{
			_blinkTimer.Dispose();
		}
	}
}