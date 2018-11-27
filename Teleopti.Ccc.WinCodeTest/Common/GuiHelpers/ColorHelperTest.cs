using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
	[TestFixture]
	public class ColorHelperTest
	{
		private ColorHelper _helper;

		[Test]
		public void VerifyingColors()
		{
			Assert.AreEqual(Color.DarkRed, ColorHelper.WarningButtonColor);
			Assert.AreEqual(Color.WhiteSmoke, ColorHelper.ChartSettingsDisabledColor);
			Assert.AreEqual(Color.FromArgb(75, 75, 75), ColorHelper.LinkColor);
			Assert.AreEqual(Color.FromArgb(215, 232, 251), ColorHelper.OfficeBlue);
			Assert.AreEqual(LinkBehavior.NeverUnderline, ColorHelper.LinkBehavior);
			Assert.AreEqual(Color.FromArgb(178, 209, 252), ColorHelper.TrackBarGradientEndColor);
			Assert.AreEqual(Color.FromArgb(240, 247, 255), ColorHelper.TrackBarGradientStartColor);
			Assert.AreEqual(Color.WhiteSmoke, ColorHelper.DisabledCellColor);
			Assert.AreEqual(Color.MistyRose, ColorHelper.UnsavedDayColor);
			Assert.AreEqual(Color.MediumBlue,ColorHelper.GridControlGridCellOldStandardTemplate());
			Assert.AreEqual(new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.SkyBlue), ColorHelper.OverstaffingBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.SkyBlue), ColorHelper.InvalidDistributionBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.Yellow), ColorHelper.WarningBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.FromArgb(128, 255, 255, 0)), ColorHelper.UnderstaffingBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.FromArgb(128, 255, 0, 0)), ColorHelper.SeriousOverstaffingBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(128, 255, 0, 0)), ColorHelper.BudgetUnderstaffingBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.Vertical, Color.FromArgb(153, 153, 153), Color.FromArgb(153, 153, 153)), ColorHelper.ReadOnlyBackgroundBrush);
			Assert.AreEqual(new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.FromArgb(128, 255, 0, 0)), ColorHelper.IntervalIssueBrush);
			Assert.AreEqual(Color.White, ColorHelper.DialogBackColor());
			Assert.AreEqual(new Font("Arial", 10), ColorHelper.ScheduleViewBaseCellFontBig);
			Assert.AreEqual(new Font("Arial", 8), ColorHelper.ScheduleViewBaseCellFontSmall);
			Assert.AreEqual(new Font("Courier New", 10), ColorHelper.ScheduleViewBaseCellFontTimeline);
			Assert.AreEqual(Color.AntiqueWhite, ColorHelper.ScheduleViewBaseHolidayCell);
			Assert.AreEqual(Color.FromArgb(200, Color.SandyBrown), ColorHelper.ScheduleViewBaseHolidayCellNotPublished);
			Assert.AreEqual(Color.SandyBrown, ColorHelper.ScheduleViewBaseCellNotPublished);
			Assert.AreEqual(Color.FromArgb(255, 240, 165, 20), ColorHelper.ScheduleViewBaseHolidayHeader);
			Assert.AreEqual(Color.White, ColorHelper.AlarmsNoColorSelected);
			Assert.AreEqual(Color.Orange, ColorHelper.WarningTextColor);
			Assert.AreEqual(Color.Red, ColorHelper.ErrorTextColor);
			Assert.AreEqual(Color.Black, ColorHelper.DefaultTextColor);
			Assert.IsNotNull(ColorHelper.RibbonFormBaseColor());
			Assert.IsNotNull(ColorHelper.ControlGradientPanelBrush());
			Assert.IsNotNull(ColorHelper.TabBackColor());
			Assert.IsNotNull(ColorHelper.TabForegroundColor());
			Assert.IsNotNull(ColorHelper.GridControlGridInteriorColor());
			Assert.IsNotNull(ColorHelper.GridControlGridExteriorColor());
			Assert.IsNotNull(ColorHelper.XPTaskbarHeaderColor());
			Assert.IsNotNull(ColorHelper.XPTaskbarBackgroundColor());
			Assert.IsNotNull(ColorHelper.ChartControlBackColor());
			Assert.IsNotNull(ColorHelper.ChartControlBackInterior());
			Assert.IsNotNull(ColorHelper.ChartControlChartAreaBackInterior());
			Assert.IsNotNull(ColorHelper.ChartControlChartInterior());
			Assert.IsNotNull(ColorHelper.BorderStyle());
			Assert.IsNotNull(ColorHelper.ControlPanelColor);
			Assert.IsNotNull(ColorHelper.RibbonContextTabColor());
			Assert.IsNotNull(ColorHelper.GridControlGridHolidayCellColor());
			Assert.IsNotNull(ColorHelper.GridControlGridHolidayHeaderColor());
			Assert.IsNotNull(ColorHelper.GridControlGridCellNoTemplate());
			Assert.IsNotNull(ColorHelper.GridControlGridCellStandardTemplate());
			Assert.IsNotNull(ColorHelper.GridControlGridCellSpecialTemplate());
			Assert.IsNotNull(ColorHelper.WizardPanelBackgroundColor());
			Assert.IsNotNull(ColorHelper.WizardBackgroundColor());
			Assert.IsNotNull(ColorHelper.WarningSkillIsUsedColor());
			Assert.IsNotNull(ColorHelper.StandardPanelBackground());
			Assert.IsNotNull(ColorHelper.FormBackgroundColor());
			Assert.IsNotNull(ColorHelper.StandardContextMenuColor());
			Assert.IsNotNull(ColorHelper.StandardTreeBackgroundColor());
			Assert.IsNotNull(ColorHelper.WizardPanelButtonHolder());
			Assert.IsNotNull(ColorHelper.WizardPanelSeparator());
			Assert.IsNotNull(ColorHelper.StandardTreeSelectedItemColor());
			Assert.IsNotNull(ColorHelper.GridControlOutlierColor());
			Assert.IsNotNull(ColorHelper.GridControlIncomingColor());
			Assert.IsNotNull(ColorHelper.ChartMargins());
			Assert.IsNotNull(ColorHelper.GridHeaderColumnWidth());
			Assert.IsNotNull(ColorHelper.PortalTreeViewSeparator());
			Assert.IsNotNull(ColorHelper.SelectedTreeViewNodeBackColor());
			Assert.IsNotNull(ColorHelper.SelectedTreeViewNodeForeColor());
			Assert.IsNotNull(ColorHelper.DialogControlHeader());
			Assert.IsNotNull(ColorHelper.OptionsDialogHeaderBackColor());
			Assert.IsNotNull(ColorHelper.OptionsDialogSubHeaderBackColor());
			Assert.IsNotNull(ColorHelper.OptionsDialogHeaderForeColor());
			Assert.IsNotNull(ColorHelper.GridControlSortingArrow);
			Assert.IsNotNull(ColorHelper.IntradayGridTimelineColor);
			Assert.IsNotNull(ColorHelper.ScheduleControlClickedItemBorder);
			Assert.IsNotNull(ColorHelper.AgentPortalScheduleCtrlVisualStyle);
			Font font = new Font("Arial", 10, FontStyle.Italic);
			Assert.AreEqual(font, ColorHelper.ChangeInfoTextFontStyleItalic(font));
			Assert.IsNotNull(ColorHelper.ChangeInfoTextColor());
			Assert.IsNotNull(ColorHelper.WizardHeaderBrush);

			_helper = new ColorHelper();
			Assert.IsNotNull(_helper.GetActiveControl(new Control()));
			TimeSpan timeSpan = new TimeSpan();
			Assert.IsTrue(ColorHelper.TryParseSecondsTimeSpan("00:00:02", out timeSpan));
			Assert.IsTrue(ColorHelper.TryParseSecondsTimeSpan("1,1", out timeSpan));
			Assert.IsFalse(ColorHelper.TryParseSecondsTimeSpan("Kalle", out timeSpan));
			Assert.IsNotNull(ColorHelper.CreateTabPage("TabbPage", "Fin tab"));
			Assert.IsNotNull(ColorHelper.OptionsDialogSubHeaderForeColor());
			
		}

		[Test]
		public static void VerifySetQuickAccess()
		{
			RibbonControlAdv ribbonControl = new RibbonControlAdv();
			ColorHelper.SetRibbonQuickAccessTexts(ribbonControl);

			Assert.AreEqual(ribbonControl.SystemText.QuickAccessDialogOkText, UserTexts.Resources.Ok);

		}


		[Test]
		public static void VerifySetThemes()
		{

			var tabControl = new TabControlAdv();
			var splitContainer = new SplitContainerAdv();

			Assert.AreNotEqual(new Font("Tahoma", 8.25F), splitContainer.Font);
			ColorHelper.SetSplitContainerTheme(splitContainer);
			Assert.AreEqual(new Font("Tahoma", 8.25F), splitContainer.Font);
			Assert.AreNotEqual(new Font("Tahoma", 8.25F), tabControl.ActiveTabFont);
			ColorHelper.SetTabControlTheme(tabControl);
			Assert.AreEqual(new Font("Segoe UI", 9F), tabControl.ActiveTabFont);
			ColorHelper.SetTabControlSettings(tabControl);
			Assert.AreEqual(Color.Red, tabControl.InactiveTabColor);

		}


	}
}
