using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers
{
	public class ColorHelper
	{
		private static readonly ThemeSettings MySettings = ThemeSettings.Default;

		public static Color WarningButtonColor
		{
			get { return Color.DarkRed; }
		}

		public static Color ChartSettingsDisabledColor
		{
			get { return Color.WhiteSmoke; }
		}

		public static Color LinkColor
		{
			get { return Color.FromArgb(75, 75, 75); }
		}

		public static Color OfficeBlue
		{
			get { return Color.FromArgb(215, 232, 251); }
		}

		public static LinkBehavior LinkBehavior
		{
			get { return LinkBehavior.NeverUnderline; }
		}

		public static Color TrackBarGradientEndColor
		{
			get { return Color.FromArgb(178, 209, 252); }
		}

		public static Color TrackBarGradientStartColor
		{
			get { return Color.FromArgb(240, 247, 255); }
		}

		public static BrushInfo OverstaffingBrush
		{
			get { return new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.SkyBlue); } //Blue
		}

		public static BrushInfo InvalidDistributionBrush
		{
			get { return new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.SkyBlue); } //Blue
		}
		
		public static BrushInfo WarningBrush
		{
			get { return new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.Yellow); } 
		}

		public static Color DisabledCellColor
		{
			get { return Color.WhiteSmoke; } //Gray
		}

		public static BrushInfo UnderstaffingBrush
		{
			get { return new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.FromArgb(128, 255, 255, 0)); } //Yellow
		}

		public static BrushInfo SeriousOverstaffingBrush
		{
			get { return new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.FromArgb(128, 255, 0, 0)); } //Red
		}

		public static BrushInfo IntervalIssueBrush
		{
			get { return new BrushInfo(GradientStyle.BackwardDiagonal, Color.White, Color.FromArgb(128, 255, 0, 0)); } //Red
		}

		public static BrushInfo BudgetUnderstaffingBrush
		{
			get { return new BrushInfo(GradientStyle.Vertical, Color.White, Color.FromArgb(128, 255, 0, 0)); } //Red
		}

		public static BrushInfo ReadOnlyBackgroundBrush
		{
			get { return new BrushInfo(GradientStyle.Vertical, Color.FromArgb(153, 153, 153), Color.FromArgb(153, 153, 153)); } 
		}

		public static Color DialogBackColor()
		{
			return Color.White;
		}

		public static Color UnsavedDayColor
		{
			get { return Color.MistyRose; }
		}

		#region SchedulViewBase

		public static Font ScheduleViewBaseCellFontBig
		{
			get { return new Font("Arial", 10); }
		}
		public static Font ScheduleViewBaseCellFontSmall
		{
			get { return new Font("Arial", 8); }
		}
		public static Font ScheduleViewBaseCellFontTimeline
		{
			get { return new Font("Courier New", 10); }
		}
		public static Color ScheduleViewBaseHolidayCell
		{
			get { return Color.AntiqueWhite; }
		}
		public static Color ScheduleViewBaseHolidayCellNotPublished
		{
			get { return Color.FromArgb(200, Color.SandyBrown); }
		}
		public static Color ScheduleViewBaseCellNotPublished
		{
			get { return Color.SandyBrown; }
		}
		public static Color ScheduleViewBaseHolidayHeader
		{
			get { return Color.FromArgb(255, 240, 165, 20); }
		}

		public static Color AlarmsNoColorSelected
		{
			get { return Color.White; }
		}

		#endregion

		private const GradientStyle Vertical = GradientStyle.Vertical;

		public static Color RibbonFormBaseColor()
		{
			return MySettings.RibbonBaseColor;
		}

		public static BrushInfo ControlGradientPanelBrush()
		{

			return new BrushInfo(Vertical, MySettings.StandardForegroundColor, MySettings.GradientPanelBackgroundColor);
		}

		public static Color TabBackColor()
		{
			return MySettings.TabBackgroundColor;
		}
		public static Color TabForegroundColor()
		{
			//or what color?
			return MySettings.TabBackgroundColor;
		}

		public static Color GridControlGridInteriorColor()
		{
			return Color.White;//mySettings.GridInteriorColor;
		}
		public static Color GridControlGridExteriorColor()
		{
			return Color.White;//mySettings.GridExteriorColor;
		}
		public static Color XPTaskbarHeaderColor()
		{
			return MySettings.XptaskBarHeaderColor;
		}

		public static Color XPTaskbarBackgroundColor()
		{
			return MySettings.XptaskBarBackground;
		}
		public static Color ChartControlBackColor()
		{
			return MySettings.ChartControlBackColor;
		}
		public static BrushInfo ChartControlBackInterior()
		{
			return new BrushInfo(GradientStyle.Vertical, MySettings.ChartControlBackInteriorForeColor, MySettings.ChartControlBackInteriorBackColor);

		}
		public static BrushInfo ChartControlChartAreaBackInterior()
		{
			return new BrushInfo(GradientStyle.Vertical, MySettings.ChartAreaBackInteriorForeColor, MySettings.ChartAreaBackInteriorBackColor);
		}

		public static BrushInfo ChartControlChartInterior()
		{
			return new BrushInfo(GradientStyle.Vertical, MySettings.ChartControlChartInteriorForeColor, MySettings.ChartControlChartInteriorBackColor);
		}
		/// <summary>
		/// Returns a border style
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-06-11
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public static BorderStyle BorderStyle()
		{
			return System.Windows.Forms.BorderStyle.None;
		}

		public static Color ControlPanelColor
		{
			get
			{
				return Color.FromArgb(216, 228, 246);
			}
		}


		private int _stopIndex;



		/// <summary>
		/// Gets the active control (goes inside ContainerControls to find the actual active control).
		/// </summary>
		/// <param name="theControl">The control (typically Form or Forms ActiveControl.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-03-10
		/// </remarks>
		public Control GetActiveControl(Control theControl)
		{
			var containerControl = theControl as ContainerControl;
			if (containerControl != null &&
				_stopIndex < 10)
			{
				_stopIndex++;
				return containerControl.ActiveControl != null ? GetActiveControl(containerControl.ActiveControl) : containerControl; //returns containercontrol if containerControl.ActiveControl == null //fix for bug:3368 Peter
			}
			_stopIndex = 0;
			return theControl;
		}

		public static bool TryParseSecondsTimeSpan(string content, out TimeSpan value)
		{
			TimeSpan timeResult;
			if (TimeSpan.TryParse(content, out timeResult))
			{
				value = timeResult;
				return true;
			}

			double seconds;
			if (double.TryParse(content, NumberStyles.Number, CultureInfo.CurrentCulture, out seconds))
			{
				value = TimeSpan.FromSeconds(seconds);
				return true;
			}

			value = TimeSpan.Zero;
			return false;
		}

		//public static Color GetCurrentDayColor()
		//{
		//    return mySettings.ForecasterTemplateNaturalDayButton;
		//}

		public static Color RibbonContextTabColor()
		{
			return MySettings.RibbonContextTabColor;
		}

		public static Color GridControlGridCellOldStandardTemplate()
		{
			return Color.MediumBlue;
		}

		public static Color GridControlGridHolidayCellColor()
		{
			return MySettings.GridHolidayCellColor;
		}

		public static Color GridControlGridHolidayHeaderColor()
		{
			return MySettings.GridHolidayHeaderColor;
		}

		public static Color GridControlGridCellNoTemplate()
		{
			return Color.IndianRed;
		}

		public static Color GridControlGridCellStandardTemplate()
		{
			return Color.ForestGreen;
		}

		public static Color GridControlGridCellSpecialTemplate()
		{
			return Color.Black;
		}

		public static Color WizardPanelBackgroundColor()
		{
			return Color.White;
		}

		public static Color WizardBackgroundColor()
		{
			return Color.White;
		}
		
		public static Color WarningSkillIsUsedColor()
		{
			return Color.Red;
		}

		public static Color WarningTextColor
		{
			get { return Color.Orange; }
		}
		
		public static Color ErrorTextColor
		{
			get { return Color.Red; }
		}
		
		public static Color DefaultTextColor
		{
			get { return Color.Black; }
		}

		//todo: another setting in ForecasterSettings
		public static Color StandardPanelBackground()
		{
			return MySettings.StandardPanelBackgroundColor;
		}
		//todo: another setting in ForecasterSettings
		public static Color FormBackgroundColor()
		{
			return Color.White;
		}

		public static Color StandardContextMenuColor()
		{
			return MySettings.StandardContextMenuBackgroundColor;
		}

		public static Color StandardTreeBackgroundColor()
		{
			return Color.White;
		}

		public static TabPageAdv CreateTabPage(string name, string description)
		{
			var tabPage = new TabPageAdv(name)
							  {
								  ToolTipText = description,
								  ThemesEnabled = true,
								  BackColor = MySettings.GradientPanelBackgroundColor,
								  Dock = DockStyle.Fill
							  };
			return tabPage;
			
		}

		//public static ProgressControl InitializeProgressControl(int stepMaximum)
		//{
		//    ProgressControl pro = new ProgressControl();
		//    pro.StepLength = 1;
		//    pro.StepMaximum = stepMaximum;
		//    pro.Show();
		//    return pro;
		//}

		public static void SetRibbonQuickAccessTexts(RibbonControlAdv ribbonControl)
		{
			ribbonControl.SystemText.QuickAccessAddItemText = Resources.AddToQuickAccessToolBar;
			ribbonControl.SystemText.QuickAccessCustomizeCaptionText = Resources.CustomizeQuickAccessToolbar;
			ribbonControl.SystemText.QuickAccessCustomizeMenuText = Resources.CustomizeQuickAccessToolbarDots;
			ribbonControl.SystemText.QuickAccessDialogAddText = Resources.AddArrows;
			ribbonControl.SystemText.QuickAccessDialogCancelText = Resources.Cancel;
			ribbonControl.SystemText.QuickAccessDialogCommandsText = Resources.ChooseCommandsFromColon;
			ribbonControl.SystemText.QuickAccessDialogDropDownName = Resources.StartMenu;
			ribbonControl.SystemText.QuickAccessDialogOkText = Resources.Ok;
			ribbonControl.SystemText.QuickAccessDialogRemoveText = Resources.ArrowsRemove;
			ribbonControl.SystemText.QuickAccessDialogResetText = Resources.Reset;
			ribbonControl.SystemText.QuickAccessMinimizeRibbon = Resources.MinimizeTheRibbon;
			ribbonControl.SystemText.QuickAccessPlaceAboveText = Resources.PlaceQuickAccessToolbarAboveTheRibbon;
			ribbonControl.SystemText.QuickAccessPlaceBelowText = Resources.PlaceQuickAccessToolbarBelowTheRibbon;
			ribbonControl.SystemText.QuickAccessRemoveItemText = Resources.RemoveFromQuickAccessToolbar;
			foreach (ToolStripItem tsi in ribbonControl.Header.QuickItems)
			{
				if (tsi.GetType() == typeof(QuickButtonReflectable))
				{
					var toolStripButton = (QuickButtonReflectable)tsi;
					tsi.ToolTipText = toolStripButton.ReflectedButton.Text;
				}
				else if (tsi.GetType() == typeof(QuickDropDownButtonReflectable))
				{
					var toolStripDropDownButton = (QuickDropDownButtonReflectable)tsi;
					tsi.ToolTipText = toolStripDropDownButton.ReflectedDropDownButton.Text;
				}
			}
		}

		public static void SetTabControlTheme(TabControlAdv control)
		{
			control.ActiveTabFont = new Font("Segoe UI", 9F);
			control.ActiveTabColor = Color.FromArgb(0, 153, 255);
			control.BackColor = TabBackColor();
			control.BorderStyle = System.Windows.Forms.BorderStyle.None;
			control.BorderWidth = 0;
			control.Dock = DockStyle.Fill;
			control.FocusOnTabClick = false;
			control.HotTrack = true;
			control.ImageOffset = 10;
			control.ItemSize = new Size(79, 24);
			control.LevelTextAndImage = true;
			control.PersistTabState = true;
			control.ShowToolTips = true;
			control.TabPanelBackColor = Color.White;
			control.ThemesEnabled = true;
			control.UserMoveTabs = true;
		}

		public static void SetSplitContainerTheme(SplitContainerAdv control)
		{
			BrushInfo myBrush = ControlGradientPanelBrush();
			control.Panel1.BackgroundColor = myBrush;
			control.Panel1.Font = new Font("Tahoma", 8.25F);
			control.Panel2.BackgroundColor = myBrush;
			control.Panel2.Font = new Font("Tahoma", 8.25F);
			control.BackgroundColor = myBrush;
			control.Font = new Font("Tahoma", 8.25F);
			control.SplitterIncrement = 1;
			control.SplitterWidth = 5;
			control.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
		}
		
		public static Color WizardPanelButtonHolder()
		{
			return Color.FromArgb(225, 232, 241);
		}

		public static Color WizardPanelSeparator()
		{
			return Color.DarkGray;
		}

		public static Color StandardTreeSelectedItemColor()
		{
			return Color.FromArgb(225, 232, 241);
		}

		public static Color GridControlOutlierColor()
		{
			return Color.FromArgb(212, 255, 212);
		}

		public static Color GridControlIncomingColor()
		{
			return Color.FromArgb(212, 255, 212);
		}

		public static ChartMargins ChartMargins()
		{
			return new ChartMargins(5, 5, 5, 5);
		}

		public static int GridHeaderColumnWidth()
		{
			return 145;
		}

		public static Color PortalTreeViewSeparator()
		{
			return Color.White;
		}
		public static Color ChangeInfoTextColor()
		{
			return Color.DarkGray;
		}
		public static Font ChangeInfoTextFontStyleItalic(Font font)
		{
			return new Font(font.FontFamily, font.SizeInPoints, FontStyle.Italic);
		}

		public static Color SelectedTreeViewNodeBackColor()
		{
			return Color.FromArgb(51, 153, 255);
		}

		public static Color SelectedTreeViewNodeForeColor()
		{
			return Color.White;
		}

		public static Color DialogControlHeader()
		{
			return Color.LightSteelBlue;
		}
		
		public static Color OptionsDialogHeaderBackColor()
		{
			return Color.White;
		}

		public static Color OptionsDialogSubHeaderBackColor()
		{
			return Color.FromArgb(128,191,234);
		}

		public static Color OptionsDialogSubHeaderForeColor()
		{
			return Color.FromArgb(64, 64, 64);
		}

		public static Color OptionsDialogHeaderForeColor()
		{
		    return Color.FromArgb(0, 153, 255);
		}

		public static Color GridControlSortingArrow
		{
			get { return Color.MidnightBlue; }
		}

		public static Color IntradayGridTimelineColor
		{
			get { return Color.DarkRed; }
		}

		public static Color ScheduleControlClickedItemBorder
		{
			get { return Color.DarkRed; }
		}

		public static GridVisualStyles AgentPortalScheduleCtrlVisualStyle
		{
			get { return GridVisualStyles.Office2007Blue;  }
		}

		public static BrushInfo WizardHeaderBrush
		{
			get { return new BrushInfo(GradientStyle.Vertical, Color.White, WizardPanelButtonHolder()); }
		}

		
		public static void SetTabControlSettings(TabControlAdv theTab)
		{
			//for some strange reason this works but only in this order....
			theTab.Office2007ColorScheme = Office2007Theme.Black;
			theTab.ForeColor = Color.Black;

			theTab.TabPanelBackColor = Color.Red;
			theTab.BackColor = Color.Gray;
			theTab.ActiveTabColor = Color.Red;
			theTab.InactiveTabColor = Color.Red;

			theTab.ThemesEnabled = false; 
		}
	}
}