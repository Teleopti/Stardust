using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsPreferencePainter : PreferenceCellPainterBase
	{
		private readonly AgentRestrictionsPreferencePainterDecider _decider;

		public AgentRestrictionsPreferencePainter(GridControlBase gridControlBase) : base(gridControlBase)
		{
			_decider = new AgentRestrictionsPreferencePainterDecider();	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
		public override bool CanPaint(IPreferenceCellData preferenceCellData)
		{
			return _decider.ShouldPaint(preferenceCellData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
		public override void Paint(Graphics g, Rectangle clientRectangle, IPreferenceCellData preferenceCellData, PreferenceRestriction preference, IEffectiveRestriction effectiveRestriction, StringFormat format)
        {
			if (_decider.ShouldPaintPreferredDayOff(preferenceCellData)) PaintPrefferedDayOff(g, clientRectangle, effectiveRestriction, format);
			if (_decider.ShouldPaintPreferredShiftCategory(preferenceCellData)) PaintPrefferedShiftCategory(g, clientRectangle, effectiveRestriction, format);
			if (_decider.ShouldPaintPreferredAbsence(preferenceCellData)) PaintPrefferedAbsence(g, clientRectangle, effectiveRestriction, format, preferenceCellData);
			if (_decider.ShouldPaintPreferredAbsenceOnContractDayOff(preferenceCellData)) PaintPrefferedAbsenceOnContractDayOff(g, clientRectangle, format, preferenceCellData);
			if (_decider.ShouldPaintPreferredExtended(preferenceCellData)) PaintPrefferedExtended(g, clientRectangle, format);
        }

		private void PaintPrefferedDayOff(Graphics g, Rectangle clientRectangle, IEffectiveRestriction effectiveRestriction, StringFormat format)
		{
			Rectangle rect = RestrictionRect(clientRectangle);

			using (var brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, effectiveRestriction.DayOffTemplate.DisplayColor, Color.LightGray))
			{
				g.FillRectangle(brush, rect);
			}

			g.DrawString(LongOrShortText(effectiveRestriction.DayOffTemplate.Description.Name, effectiveRestriction.DayOffTemplate.Description.ShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
		}

		private void PaintPrefferedShiftCategory(Graphics g, Rectangle clientRectangle, IEffectiveRestriction effectiveRestriction, StringFormat format)
		{
			var rect = RestrictionRect(clientRectangle);
			var gradRect = new Rectangle(rect.Location, rect.Size);
			gradRect.Inflate(0, 16);
			gradRect.Offset(0, 15);
			using (var lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, effectiveRestriction.ShiftCategory.DisplayColor, 90, false))
			{
				GridHelper.FillRoundedRectangle(g, rect, 7, lBrush, 0);
			}

			g.DrawString(LongOrShortText(effectiveRestriction.ShiftCategory.Description.Name, effectiveRestriction.ShiftCategory.Description.ShortName, rect.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
		}

		private void PaintPrefferedAbsence(Graphics g, Rectangle clientRectangle, IEffectiveRestriction effectiveRestriction, StringFormat format, IPreferenceCellData preferenceCellData)
		{
			var rect = RestrictionRect(clientRectangle);
			var gradRect = new Rectangle(rect.Location, rect.Size);
			gradRect.Inflate(0, 16);
			gradRect.Offset(0, 15);

			var startSize = g.MeasureString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font);
			var paddingLength = Math.Max((rect.Width - startSize.Width) / 2, 0);

			using (var lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, effectiveRestriction.Absence.DisplayColor, 90, false))
			{
				GridHelper.FillRoundedRectangle(g, rect, 0, lBrush, 0);
			}
		
			g.DrawString(LongOrShortText(effectiveRestriction.Absence.Description.Name, effectiveRestriction.Absence.Description.ShortName, rect.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
			g.DrawString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, rect.Left + paddingLength, rect.Top + 36);
		}

		private void PaintPrefferedAbsenceOnContractDayOff(Graphics g, Rectangle clientRectangle, StringFormat format, IPreferenceCellData preferenceCellData)
		{
			var rect = RestrictionRect(clientRectangle);
			using (var lBrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.LightGray, preferenceCellData.EffectiveRestriction.Absence.DisplayColor))
			{
				GridHelper.FillRoundedRectangle(g, rect, 0, lBrush, 0);
			}

			var startSize = g.MeasureString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font);
			var startSizeStartEnd = g.MeasureString(preferenceCellData.StartEndScheduledShift, GridControlBase.Font);
			var paddingLength = Math.Max((rect.Width - startSize.Width) / 2, 0);
			var paddingStartEnd = Math.Max((rect.Width - startSizeStartEnd.Width) / 2, 0);
			g.DrawString(LongOrShortText(preferenceCellData.EffectiveRestriction.Absence.Description.Name, preferenceCellData.EffectiveRestriction.Absence.Description.ShortName, rect.Width, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
			format.Alignment = StringAlignment.Center;

			g.DrawString(preferenceCellData.StartEndScheduledShift, GridControlBase.Font, Brushes.Black, rect.Left + paddingStartEnd, rect.Top + 24);
			g.DrawString(preferenceCellData.ShiftLengthScheduledShift, GridControlBase.Font, Brushes.Black, rect.Left + paddingLength, rect.Top + 36);
		}

		private void PaintPrefferedExtended(Graphics g, Rectangle clientRectangle , StringFormat format)
		{
			var rect = RestrictionRect(clientRectangle);
			var gradRect = new Rectangle(rect.Location, rect.Size);
			gradRect.Inflate(0, 16);
			gradRect.Offset(0, 15);
			using (var lBrush = new LinearGradientBrush(gradRect, Color.WhiteSmoke, Color.LightGray, 90, false))
			{
				GridHelper.FillRoundedRectangle(g, rect, 7, lBrush, 0);
			}
			g.DrawString(LongOrShortText(UserTexts.Resources.Extended, UserTexts.Resources.ExtDot, rect.Width - 8, g), GridControlBase.Font, Brushes.Black, MiddleX(rect), MiddleY(rect) - 7, format);
		}
	}
}
