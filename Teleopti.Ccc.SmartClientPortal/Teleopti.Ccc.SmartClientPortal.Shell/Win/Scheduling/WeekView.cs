using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class WeekView : ScheduleViewBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public WeekView(GridControl grid, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
            : base(grid, timeZoneGuard)
        {
            Presenter = new WeekPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer)
                            {VisibleWeeks = 1};
            grid.Name = "WeekView";
        }

        protected override int CellWidth()
        {
            return 160;
        }

        //draw cell
        internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (e.RowIndex > 1 && e.ColIndex >=(int)ColumnType.StartScheduleColumns)
            {
                var scheduleRange = e.Style.CellValue as IScheduleDay;
                if (scheduleRange != null)
                {
                    var significantPart = scheduleRange.SignificantPartForDisplay();
                    DrawSchedule(e, WeekPresenter.CreateSpanDictionaryFromSchedule(scheduleRange),significantPart);
                    drawInfoTextInCell(e, scheduleRange, significantPart);
                    AddMarkersToCell(e, scheduleRange, significantPart);
                    addWeekViewMarkersToCell(e, scheduleRange);
                }
            }
        }

        private void drawInfoTextInCell(GridDrawCellEventArgs e, IScheduleDay scheduleRange, SchedulePartView significantPart)
        {
            IList<string> infoList = ViewBaseHelper.GetInfoTextWeekView(scheduleRange, significantPart);
            string infoText = infoList[0];
            string periodText = infoList[1];
            string timeText = infoList[2];

            if (!string.IsNullOrEmpty(infoText))
            {
                SizeF stringWidth1 = e.Graphics.MeasureString(infoText, CellFontSmall);
                SizeF stringWidth2 = e.Graphics.MeasureString(periodText, CellFontSmall);
                SizeF stringWidth3 = e.Graphics.MeasureString(timeText, CellFontSmall);

                var point1 =
                    new Point(e.Bounds.X - (int)stringWidth1.Width / 2 + e.Bounds.Width / 2,
                              e.Bounds.Y + 2);

                var point2 =
                    new Point(e.Bounds.X - (int)stringWidth2.Width / 2 + e.Bounds.Width / 2,
                              point1.Y + (int)stringWidth1.Height + 2);

                var point3 =
                    new Point(e.Bounds.X - (int)stringWidth3.Width / 2 + e.Bounds.Width / 2,
                              point2.Y + (int)stringWidth2.Height + 2);

                e.Graphics.DrawString(infoText, CellFontSmall, Brushes.Black, point1);
                e.Graphics.DrawString(periodText, CellFontSmall, Brushes.Black, point2);
                e.Graphics.DrawString(timeText, CellFontSmall, Brushes.Black, point3);
            }
        }

        private static void addWeekViewMarkersToCell(GridDrawCellEventArgs e, IScheduleDay schedulePart)
        {
            var restrictionChecker = new RestrictionChecker();

            PermissionState permissionState = restrictionChecker.CheckAvailability(schedulePart);
            var drawRestrictionIcon = new DrawRestrictionIcon(e);
            drawRestrictionIcon.DrawAvailability(permissionState);

            permissionState = restrictionChecker.CheckRotations(schedulePart);
            drawRestrictionIcon.DrawRotation(permissionState);

			permissionState = checkPreferenceForDisplay(schedulePart, restrictionChecker);

			var mustHavePreference = false;
	        if (permissionState != PermissionState.None)
	        {
		        mustHavePreference = restrictionChecker.HaveMustHavePreference(schedulePart);
	        }

			drawRestrictionIcon.DrawPreference(permissionState, mustHavePreference);

			var personPeriod = schedulePart.Person.Period(schedulePart.DateOnlyAsPeriod.DateOnly);
			if (personPeriod?.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)
			{
				permissionState = restrictionChecker.CheckStudentAvailability(schedulePart);
				var isAnyAvailabilityLeftToUse = false;
				if(permissionState == PermissionState.Satisfied)
				{
					if (new HourlyAvailiabilityDisplayHelper().IsAnyAvailabilityLeftToUse(schedulePart))
						isAnyAvailabilityLeftToUse = true;
				}
				drawRestrictionIcon.DrawStudentAvailability(permissionState, isAnyAvailabilityLeftToUse);
			}
		}
		
		private static PermissionState checkPreferenceForDisplay(IScheduleDay schedulePart, RestrictionChecker restrictionChecker)
		{
			var preferenceDay = schedulePart?.PreferenceDay();
			if (preferenceDay?.Restriction == null) 
				return PermissionState.None;
			var permissionState = restrictionChecker.CheckPreferenceDayOffForDisplay(schedulePart);
			if (permissionState != PermissionState.Broken) 
				permissionState = restrictionChecker.CheckPreferenceAbsence(permissionState, schedulePart);
			if (permissionState != PermissionState.Unspecified && permissionState != PermissionState.Satisfied)
				return permissionState;
			var permissionStateShift = restrictionChecker.CheckPreferenceShift(schedulePart);
			if (permissionStateShift == PermissionState.Broken || permissionStateShift == PermissionState.Satisfied) 
				permissionState = permissionStateShift;

			return permissionState;
		} 

        internal override void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            if(e.Index > 1)
            {
                e.Size = 70;
                e.Handled = true;
            }
            else
            {
                e.Size = 22;
                e.Handled = true;
            }
        }
    }
}
