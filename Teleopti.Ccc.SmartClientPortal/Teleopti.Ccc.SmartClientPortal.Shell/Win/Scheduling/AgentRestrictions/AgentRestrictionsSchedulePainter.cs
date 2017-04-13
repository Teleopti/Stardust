using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsSchedulePainter : PreferenceCellPainterBase
	{
		private readonly AgentRestrictionsSchedulePainterDecider _decider;
		private Rectangle _restrictionRectangle;

		public AgentRestrictionsSchedulePainter(GridControlBase gridControlBase) : base(gridControlBase)
		{
			_decider = new AgentRestrictionsSchedulePainterDecider();	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		public override bool CanPaint(IPreferenceCellData preferenceCellData)
		{
			return _decider.ShouldPaint(preferenceCellData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
		public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData preferenceCellData, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
			_restrictionRectangle = RestrictionRect(clientRectangle);

			if(_decider.ShouldPaintFullDayAbsence(preferenceCellData)) PaintFullDayAbsence(g, clientRectangle, preferenceCellData, format);
			else if (_decider.ShouldPaintFullDayAbsenceOnContractDayOff(preferenceCellData))PaintFullDayAbsence(g, clientRectangle, preferenceCellData, format);
			else if (_decider.ShouldPaintDayOff(preferenceCellData)) PaintDayOff(g, preferenceCellData, format);
			else if (_decider.ShouldPaintMainShift(preferenceCellData)) PaintMainShift(g, preferenceCellData, format);
        }

		private void PaintFullDayAbsence(Graphics g, Rectangle clientRectangle, IPreferenceCellData preferenceCellData, StringFormat format)
		{
			var startSize = g.MeasureString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font);
			var startSizeStartEnd = g.MeasureString(preferenceCellData.StartEndScheduledShift, GridControlBase.Font);
			var paddingLength = Math.Max((_restrictionRectangle.Width - startSize.Width) / 2, 0);
			var paddingStartEnd = Math.Max((_restrictionRectangle.Width - startSizeStartEnd.Width) / 2, 0);
			
			if(_decider.ShouldPaintFullDayAbsenceOnContractDayOff(preferenceCellData))
			{
				
				using (var lBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, preferenceCellData.DisplayColor))
				{
					GridHelper.FillRoundedRectangle(g, _restrictionRectangle, 0, lBrush, 0);
				}					
			}

			else
			{
				using (var lBrush = new LinearGradientBrush(clientRectangle, Color.WhiteSmoke, preferenceCellData.DisplayColor, 90, false))
				{
					GridHelper.FillRoundedRectangle(g, _restrictionRectangle, 0, lBrush, 0);
				}
			}

			g.DrawString(LongOrShortText(preferenceCellData.DisplayName, preferenceCellData.DisplayShortName, _restrictionRectangle.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(_restrictionRectangle), MiddleY(_restrictionRectangle) - 7, format);
			format.Alignment = StringAlignment.Center;
			g.DrawString(preferenceCellData.StartEndScheduledShift, GridControlBase.Font, Brushes.Black, _restrictionRectangle.Left + paddingStartEnd, _restrictionRectangle.Top + 24);
			g.DrawString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, _restrictionRectangle.Left + paddingLength, _restrictionRectangle.Top + 36);
		}

		private void PaintDayOff(Graphics g, IPreferenceCellData preferenceCellData, StringFormat format)
		{
			var startSize = g.MeasureString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font);
			var paddingLength = Math.Max((_restrictionRectangle.Width - startSize.Width) / 2, 0);

			using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, preferenceCellData.DisplayColor, Color.LightGray))
			{
				g.FillRectangle(brush, _restrictionRectangle);
			}

			g.DrawString(LongOrShortText(preferenceCellData.DisplayName, preferenceCellData.DisplayShortName, _restrictionRectangle.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(_restrictionRectangle), MiddleY(_restrictionRectangle) - 7, format);
			g.DrawString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, _restrictionRectangle.Left + paddingLength, _restrictionRectangle.Top + 24);	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "g")]
		public void PaintMainShift(Graphics g,  IPreferenceCellData preferenceCellData , StringFormat format)
		{
			var gradRect = new Rectangle(_restrictionRectangle.Location, _restrictionRectangle.Size);
			gradRect.Inflate(0, 16);
			gradRect.Offset(0, 15);

			var startSize = g.MeasureString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font);
			var startSizeStartEnd = g.MeasureString(preferenceCellData.StartEndScheduledShift, GridControlBase.Font);
			var paddingLength = Math.Max((_restrictionRectangle.Width - startSize.Width) / 2, 0);
			var paddingStartEnd = Math.Max((_restrictionRectangle.Width - startSizeStartEnd.Width) / 2, 0);

			using (var lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, preferenceCellData.DisplayColor, 90, false))
			{
				GridHelper.FillRoundedRectangle(g, _restrictionRectangle, 7, lBrush, 0);
			}

			g.DrawString(LongOrShortText(preferenceCellData.DisplayName, preferenceCellData.DisplayShortName, _restrictionRectangle.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(_restrictionRectangle), MiddleY(_restrictionRectangle) - 7, format);
			g.DrawString(preferenceCellData.StartEndScheduledShift, GridControlBase.Font, Brushes.Black, _restrictionRectangle.Left + paddingStartEnd, _restrictionRectangle.Top + 24);
			g.DrawString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, _restrictionRectangle.Left + paddingLength, _restrictionRectangle.Top + 36);
		}
	}
}
