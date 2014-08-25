using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class SkillTabRenderer : TabRendererMetro
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
			((TabControlAdv)TabControl).TabPanelBackColor = Color.White;
			((TabControlAdv)TabControl).Font = new Font("Segoe UI", 8.25F,FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
			((TabControlAdv)TabControl).ActiveTabFont = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void DrawBackground(DrawTabEventArgs drawItemInfo)
		{
			
			var tab = ((TabControlAdv)TabControl).TabPages[drawItemInfo.Index];
			Brush bgSelectedTab = new SolidBrush(Color.FromArgb(102,102,102));
			Brush bgInactiveTab = new SolidBrush(Color.White);
	        if (tab.Name == "Pinned")
	        {
		        var bgColor = Color.FromArgb(110, 202, 171);
		        if ((drawItemInfo.State & DrawItemState.Selected) > 0)
			        bgColor = Color.FromArgb(145, 238, 180);
		        Brush pinnedBrush = new SolidBrush(bgColor);
		        drawItemInfo.Graphics.FillRectangle(pinnedBrush, drawItemInfo.Bounds);
		        pinnedBrush.Dispose();
	        }
	        else
	        {
				  if ((drawItemInfo.State & DrawItemState.Selected) > 0)
				  {
					  drawItemInfo.Graphics.FillRectangle(bgSelectedTab, drawItemInfo.Bounds);
					  bgSelectedTab.Dispose();
				  }
				  else
				  {
					  drawItemInfo.Graphics.FillRectangle(bgInactiveTab, drawItemInfo.Bounds);
					  bgInactiveTab.Dispose();
				  }
	        }
			
		}
	}

	public class SkillTabPanelProperty : TabUIDefaultProperties
	{
	}
}