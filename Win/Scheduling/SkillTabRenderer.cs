using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class SkillTabRenderer : TabRendererOffice2007
	{
		static readonly SkillTabPanelProperty TabPropertyExtender;
		public static new SkillTabPanelProperty TabPanelPropertyExtender
		{
			get { return TabPropertyExtender; }
		}

		public static new string TabStyleName
		{
			get { return "SkillTabRenderer"; }
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SkillTabRenderer()
		{
			TabPropertyExtender = new SkillTabPanelProperty();
			TabRendererFactory.RegisterTabType(TabStyleName, typeof(SkillTabRenderer), TabPanelPropertyExtender);
		}

		public SkillTabRenderer(ITabControl parent, ITabPanelRenderer panelRenderer)
			: base(parent, panelRenderer)
		{
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void DrawBackground(DrawTabEventArgs drawItemInfo)
		{
			
			Graphics g = drawItemInfo.Graphics;

			var tab = ((TabControlAdv)TabControl).TabPages[drawItemInfo.Index];
			var bgColor = Color.FromArgb(199,216,237);
			if ((drawItemInfo.State & DrawItemState.Selected) > 0)
				bgColor = Color.FromArgb(228, 237, 248);
			if (tab.Name == "Pinned")
			{
				bgColor = Color.FromArgb(110, 202, 171);
				if ((drawItemInfo.State & DrawItemState.Selected) > 0)
					bgColor = Color.FromArgb(210, 238, 229);
			}

			var curBounds = new RectangleF(drawItemInfo.Bounds.Left,
				drawItemInfo.Bounds.Top, drawItemInfo.Bounds.Width, drawItemInfo.Bounds.Height);

			
			GraphicsPath path = GetBorderPathFromBounds(curBounds);

			var brush = new SolidBrush(bgColor);
            try
            {
                g.FillPath(brush, path);
            }
            finally { brush.Dispose(); }
		}
	}

	public class SkillTabPanelProperty : TabUIDefaultProperties
	{
	}
}