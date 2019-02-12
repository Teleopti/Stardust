using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public static class ViewBaseHelper
	{
		public static string GetToolTip(IScheduleDay cell)
		{
			var sb = new StringBuilder();

			string assignments = GetToolTipAssignments(cell);
			string absences = GetToolTipAbsences(cell);
			string dayOff = GetToolTipDayOff(cell);
			string businessRuleConflicts = GetToolTipBusinessRuleConflicts(cell);
			string meetings = GetToolTipMeetings(cell);
			string overtime = GetToolTipOvertime(cell);

			if (assignments.Length > 0)
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(assignments);
			}

			if (absences.Length > 0)
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(absences);
			}

			if (dayOff.Length > 0)
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(dayOff);
			}

			if (meetings.Length > 0)
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(meetings);
			}
			if (overtime.Length > 0)
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(overtime);
			}

			if (businessRuleConflicts.Length > 0)
			{
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(businessRuleConflicts);
			}

			return sb.Length > 0 ? sb.ToString() : string.Empty;
		}

		/// <summary>
		/// Gets the tooltip for business rule conflicts.
		/// </summary>
		/// <param name="cell">The SchedulePart.</param>
		/// <returns></returns>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-08-25    
		/// /// </remarks>
		public static string GetToolTipBusinessRuleConflicts(IScheduleDay cell)
		{
			var sb = new StringBuilder();
			int longest = 0;
			if (cell.BusinessRuleResponseCollection.Count > 0)
			{
				foreach (BusinessRuleResponse response in cell.BusinessRuleResponseCollection)
				{
					if (sb.Length > 0) sb.AppendLine();
					if (response.Message != null && response.Message.Length > longest)
						longest = response.Message.Length;

					sb.Append(response.Message);
				}
			}

			if (sb.Length > 0)
				sb.Insert(0, new string('-', longest) + Environment.NewLine);

			return sb.ToString();
		}

		public static string ToLocalStartEndTimeString(DateTimePeriod period, TimeZoneInfo timeZoneInfo)
		{
			var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			const string separator = " - ";

			return string.Concat(TimeHelper.TimeOfDayFromTimeSpan(period.StartDateTimeLocal(timeZoneInfo).TimeOfDay, culture),
				separator,
				TimeHelper.TimeOfDayFromTimeSpan(period.EndDateTimeLocal(timeZoneInfo).TimeOfDay, culture));
		}

		public static string ToLocalStartEndTimeStringAbsences(DateTimePeriod partPeriod, DateTimePeriod absencePeriod,
			TimeZoneInfo timeZoneInfo)
		{
			var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			const string separator = " - ";
			DateTimePeriod startTimePeriod = absencePeriod;
			DateTimePeriod endTimePeriod = absencePeriod;

			if (absencePeriod.StartDateTime < partPeriod.StartDateTime)
				startTimePeriod = partPeriod;

			if (absencePeriod.EndDateTime > partPeriod.EndDateTime)
				endTimePeriod = partPeriod;

			return
				string.Concat(
					TimeHelper.TimeOfDayFromTimeSpan(startTimePeriod.StartDateTimeLocal(timeZoneInfo).TimeOfDay, culture), separator,
					TimeHelper.TimeOfDayFromTimeSpan(endTimePeriod.EndDateTimeLocal(timeZoneInfo).TimeOfDay, culture));
		}

		/// <summary>
		/// Get tooltip for assignments
		/// </summary>
		/// <param name="scheduleDay"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public static string GetToolTipAssignments(IScheduleDay scheduleDay)
		{
			var sb = new StringBuilder();
			var pa = scheduleDay.PersonAssignment();
				
			if (pa != null)
			{
				if (sb.Length > 0)
					sb.AppendLine();
				if (pa.ShiftCategory != null)
					sb.Append(pa.ShiftCategory.Description.Name); //name
				sb.Append("  ");

				var projectionPeriod = scheduleDay.ProjectionService().CreateProjection().Period();

				if (projectionPeriod.HasValue)
				{
					sb.Append(ToLocalStartEndTimeString(projectionPeriod.Value, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone())); //time
				}

				foreach (var layer in pa.PersonalActivities())
				{
					sb.AppendLine();
					sb.AppendFormat(" - {0}: ", Resources.PersonalShift);

					sb.AppendLine();
					sb.Append("    ");
					sb.Append(layer.Payload.ConfidentialDescription_DONTUSE(pa.Person).Name);
					//name
					sb.Append(": ");
					sb.Append(ToLocalStartEndTimeString(layer.Period, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone())); //time
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Get tooltip for absences
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static string GetToolTipAbsences(IScheduleDay cell)
		{
			var sb = new StringBuilder();

			IList<IPersonAbsence> abses = cell.PersonAbsenceCollection();
			if (abses.Count > 0)
			{
				foreach (IPersonAbsence pa in abses)
				{
					if (sb.Length > 0) sb.AppendLine();

					sb.Append(pa.Layer.Payload.ConfidentialDescription_DONTUSE(pa.Person).Name); //name
					sb.Append(": ");
					sb.Append(ToLocalStartEndTimeStringAbsences(cell.Period, pa.Layer.Period, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()));
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Get tooltip for meetings
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static string GetToolTipMeetings(IScheduleDay cell)
		{
			var sb = new StringBuilder();

			foreach (IPersonMeeting personMeeting in cell.PersonMeetingCollection())
			{
				if (sb.Length > 0) sb.AppendLine();

				sb.Append(personMeeting.BelongsToMeeting.GetSubject(new NoFormatting()));
				sb.Append(": ");
				sb.Append(ToLocalStartEndTimeString(personMeeting.Period, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()));

				if (personMeeting.Optional)
					sb.AppendFormat(" ({0})", Resources.Optional);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Get tooltip for overtime
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static string GetToolTipOvertime(IScheduleDay cell)
		{
			if (!dayHasOvertime(cell)) return string.Empty;

			var sb = new StringBuilder();

			var proj = cell.ProjectionService().CreateProjection();

			foreach (IVisualLayer layer in proj)
			{
				if (layer.DefinitionSet == null)
					continue;

				if (sb.Length > 0) sb.AppendLine();
				sb.Append(layer.DefinitionSet.Name);
				sb.Append(": ");
				sb.Append(layer.Payload.ConfidentialDescription_DONTUSE(cell.Person).Name);
				sb.Append(": ");
				sb.Append(ToLocalStartEndTimeString(layer.Period, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()));
			}
			return sb.ToString();
		}

		private static bool dayHasOvertime(IScheduleDay cell)
		{
			var assignment = cell.PersonAssignment();
			return assignment != null && assignment.OvertimeActivities().Any();
		}

		/// <summary>
		/// Get tooltip for absences
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static string GetToolTipDayOff(IScheduleDay cell)
		{
			var sb = new StringBuilder();
			var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			var ass = cell.PersonAssignment();
			if (ass != null)
			{
				var dayOff = ass.DayOff();
				if (dayOff != null)
				{
					sb.AppendLine(dayOff.Description.Name);
					sb.Append(Resources.TargetLengthColon + " ");
					sb.AppendLine(TimeHelper.GetLongHourMinuteTimeString(dayOff.TargetLength, culture));
					sb.Append(Resources.AnchorColon + " ");
					sb.AppendLine(
						TimeHelper.TimeOfDayFromTimeSpan(TimeZoneHelper.ConvertFromUtc(dayOff.Anchor, cell.TimeZone).TimeOfDay,
							culture));
					sb.Append(Resources.FlexibilityColon + " ");
					sb.Append(TimeHelper.GetLongHourMinuteTimeString(dayOff.Flexibility, culture));
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Return a rectangle to draw from in a timeline view
		/// </summary>
		/// <param name="periodBounds">The period bounds.</param>
		/// <param name="destinationRectangle">The destination rectangle.</param>
		/// <param name="period">The period.</param>
		/// <param name="isRightToLeft">if set to <c>true</c> [is right to left].</param>
		/// <returns></returns>
		public static Rectangle GetLayerRectangle(DateTimePeriod periodBounds, Rectangle destinationRectangle,
			DateTimePeriod period, bool isRightToLeft)
		{
			DateTimePeriod? periodIntersection = period.Intersection(periodBounds);
			if (!periodIntersection.HasValue) return new Rectangle();

			LengthToTimeCalculator calculator = new LengthToTimeCalculator(periodBounds, destinationRectangle.Width);
			Rectangle rect = calculator.RectangleFromDateTimePeriod(periodIntersection.Value,
				new Point(destinationRectangle.X, destinationRectangle.Y + 2), destinationRectangle.Height - 4, isRightToLeft);

			if (rect.Width == 0)
				return new Rectangle();

			if (!destinationRectangle.Contains(rect))
			{
				rect.Intersect(destinationRectangle);
			}

			return rect;
		}

		/// <summary>
		/// Returns the a dictionary containing first date of every selected week.
		/// </summary>
		/// <remarks>
		/// Created by: ZoeT
		/// Created date: 2007-12-05
		/// </remarks>
		public static Dictionary<int, DateOnly> AddWeekDates(DateOnlyPeriod selectedPeriod)
		{
			var firstDateOfWeek = new Dictionary<int, DateOnly>();

			foreach (var day in selectedPeriod.DayCollection())
			{
				var weekNumber = DateHelper.WeekNumber(day.Date, CultureInfo.CurrentCulture);
				if (!firstDateOfWeek.ContainsKey(weekNumber))
				{
					firstDateOfWeek.Add(weekNumber, day);
				}
			}
			return firstDateOfWeek;
		}

		/// <summary>
		/// Weeks the header dates.
		/// </summary>
		/// <param name="week">The week.</param>
		/// <param name="selectedPeriod">The selected period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2007-12-05
		/// </remarks>
		public static DateOnlyPeriod WeekHeaderDates(int week, DateOnlyPeriod selectedPeriod)
		{
			bool gotWeek = false;
			DateOnly? start = null;
			DateOnly? end = null;
			IDictionary<int, DateOnly> weekDateDictionary = AddWeekDates(selectedPeriod);
			foreach (KeyValuePair<int, DateOnly> weekDate in weekDateDictionary)
			{
				if (weekDate.Key == week && gotWeek == false)
				{
					start = weekDate.Value;
					gotWeek = true;
					continue;
				}
				if (gotWeek)
				{
					end = weekDate.Value.AddDays(-1);
					break;
				}
			}

			if (!end.HasValue) end = selectedPeriod.EndDate;
			if (!start.HasValue) start = selectedPeriod.StartDate;

			return new DateOnlyPeriod(start.Value, end.Value);

		}

		#region Style current

		public static void StyleCurrentContractTimeCell(GridStyleInfo style, IScheduleRange wholeRange, DateOnlyPeriod period)
		{
			style.CellType = "TotalTimeCell";
			style.CellValue = wholeRange.CalculatedContractTimeHolderOnPeriod(period);
		}

		public static Boolean CheckOpenPeriodMatchSchedulePeriod(IPerson person, DateOnlyPeriod openPeriod)
		{
			if (person == null)
				throw new ArgumentNullException("person");

			var virtualSchedulePeriodFirst = person.VirtualSchedulePeriod(openPeriod.StartDate);
			var virtualSchedulePeriodLast = person.VirtualSchedulePeriod(openPeriod.EndDate);

			if (virtualSchedulePeriodFirst.DateOnlyPeriod.StartDate.Equals(openPeriod.StartDate) &&
				virtualSchedulePeriodLast.DateOnlyPeriod.EndDate.Equals(openPeriod.EndDate))
				return true;

			return false;
		}

		public static void StyleCurrentTotalDayOffCell(GridStyleInfo style, IScheduleRange wholeRange, DateOnlyPeriod period)
		{
			style.CellType = "TotalDayOffCell";
			style.CellValue = wholeRange.CalculatedScheduleDaysOffOnPeriod(period);
		}

		#endregion

		#region Style target

		public static void StyleTargetScheduleContractTimeCell(GridStyleInfo style, IPerson person, DateOnlyPeriod openPeriod,
			ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleRange wholeRange)
		{

			if (style == null)
				throw new ArgumentNullException("style");

			if (wholeRange == null)
				throw new ArgumentNullException("wholeRange");

			if (!CheckOpenPeriodMatchSchedulePeriod(person, openPeriod))
			{
				setCellNotApplicable(style);
				return;
			}

			if (!CheckOverrideTargetTimeLoadedAndScheduledPeriod(person, openPeriod))
			{
				setCellNotApplicable(style);
				return;
			}

			var employmentType = person?.Period(openPeriod.StartDate).PersonContract.Contract.EmploymentType;
			if (employmentType != null && employmentType.Equals(EmploymentType.HourlyStaff))
			{
				setCellNotApplicable(style);
				return;
			}

			style.CellType = "TotalTimeCell";
			style.CellValue = wholeRange.CalculatedTargetTimeHolder(openPeriod);


		}

		public static void StyleTargetScheduleDaysOffCell(GridStyleInfo style, IPerson person, DateOnlyPeriod openPeriod,
			IScheduleRange wholeRange)
		{
			if (style == null)
				throw new ArgumentNullException("style");

			if (person == null)
				throw new ArgumentNullException("person");

			if (!CheckOpenPeriodMatchSchedulePeriod(person, openPeriod))
			{
				setCellNotApplicable(style);
				return;
			}

			if (!CheckOverrideDayOffAndLoadedAndScheduledPeriod(person, openPeriod))
			{
				setCellNotApplicable(style);
				return;
			}

			if (person.Period(openPeriod.StartDate).PersonContract.Contract.EmploymentType.Equals(EmploymentType.HourlyStaff))
			{
				setCellNotApplicable(style);
				return;
			}

			style.CellType = "TotalDayOffCell";
			style.CellValue = wholeRange.CalculatedTargetScheduleDaysOff(openPeriod);
		}


		private static void setCellNotApplicable(GridStyleInfo style)
		{
			style.CellType = "Static";
			style.CellValue = Resources.NA;
			style.HorizontalAlignment = GridHorizontalAlignment.Center;
			style.VerticalAlignment = GridVerticalAlignment.Bottom;
		}

		public static bool CheckOverrideDayOffAndLoadedAndScheduledPeriod(IPerson person, DateOnlyPeriod period)
		{
			IList<ISchedulePeriod> schedulePeriods = person.PersonSchedulePeriods(period);
			if (period.DayCollection().Select(person.VirtualSchedulePeriod).Any(vPeriod => !vPeriod.IsValid))
			{
				return false;
			}
			foreach (SchedulePeriod schedulePeriod in schedulePeriods)
			{
				if (!schedulePeriod.IsDaysOffOverride)
					return true;

				var startPeriod = schedulePeriod.GetSchedulePeriod(period.StartDate);
				if (startPeriod.HasValue && startPeriod.Value.StartDate == period.StartDate)
				{
					var endPeriod = schedulePeriod.GetSchedulePeriod(period.EndDate);
					if (endPeriod.HasValue && endPeriod.Value.EndDate == period.EndDate)
						return true;
				}
			}
			return false;
		}

		public static bool CheckOverrideTargetTimeLoadedAndScheduledPeriod(IPerson person, DateOnlyPeriod period)
		{
			var schedulePeriods = person.PersonSchedulePeriods(period);
			if (period.DayCollection().Select(person.VirtualSchedulePeriod).Any(vPeriod => !vPeriod.IsValid))
			{
				return false;
			}
			foreach (ISchedulePeriod schedulePeriod in schedulePeriods)
			{
				if (!schedulePeriod.IsAverageWorkTimePerDayOverride)
					return true;
				var startPeriod = schedulePeriod.GetSchedulePeriod(period.StartDate);
				if (startPeriod.HasValue && startPeriod.Value.StartDate == period.StartDate)
				{
					var endPeriod = schedulePeriod.GetSchedulePeriod(period.EndDate);
					if (endPeriod.HasValue && endPeriod.Value.EndDate == period.EndDate)
						return true;
				}
			}
			return false;
		}

		#endregion

		/// <summary>
		/// Gets displaymode for absence
		/// </summary>
		/// <param name="pa">The pa.</param>
		/// <param name="part">The part.</param>
		/// <returns></returns>
		public static DisplayMode GetAssignmentDisplayMode(IPeriodized pa, IScheduleDay part)
		{
			DateTimePeriod period = pa.Period;
			DateTimePeriod datePeriod = part.Period;

			DateTime layerStartDateTime = period.StartDateTimeLocal(part.TimeZone);
			DateTime layerEndDateTime = period.EndDateTimeLocal(part.TimeZone);
			DateTime periodStartDateTime = datePeriod.StartDateTimeLocal(part.TimeZone);
			DateTime periodEndDateTime = datePeriod.EndDateTimeLocal(part.TimeZone);

			if (layerStartDateTime >= periodStartDateTime && layerEndDateTime <= periodEndDateTime)
			{
				return DisplayMode.BeginsAndEndsToday;
			}
			if (layerStartDateTime < periodStartDateTime && layerEndDateTime <= periodEndDateTime)
			{
				return DisplayMode.EndsToday;
			}
			if (layerStartDateTime >= periodStartDateTime && layerEndDateTime > periodEndDateTime)
			{
				return DisplayMode.BeginsToday;
			}
			return DisplayMode.WholeDay;
		}

		//Gets text to show in week view
		public static IList<string> GetInfoTextWeekView(IScheduleDay schedulePart, SchedulePartView significantPart)
		{
			IList<string> returnList = new List<string>();
			string infoText = string.Empty;
			string periodText = string.Empty;
			string timeText = string.Empty;

			IPersonAssignment pa = schedulePart.PersonAssignment();

			if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
			{
				IVisualLayerCollection layerCollection = schedulePart.ProjectionService().CreateProjection();
				foreach (IVisualLayer layer in layerCollection)
				{
					infoText = layer.Payload.ConfidentialDescription_DONTUSE(schedulePart.Person).Name;
					break;
				}
				if (layerCollection.Count() == 0)
				{
					//we have no underlaying activity = is on top of day off
					var absenceCollection = schedulePart.PersonAbsenceCollection();
					IPersonAbsence personAbsence = absenceCollection[absenceCollection.Length - 1];
					infoText = personAbsence.Layer.Payload.Description.Name;
				}
				periodText = "-";
			}

			if (significantPart == SchedulePartView.DayOff)
			{
				infoText = schedulePart.PersonAssignment().DayOff().Description.Name;
				periodText = "-";
			}

			if (significantPart == SchedulePartView.MainShift)
			{
				var period = schedulePart.ProjectionService().CreateProjection().Period();
				infoText = pa.ShiftCategory.Description.Name;
				if (period.HasValue)
				{
					periodText = ToLocalStartEndTimeString(period.Value, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone());
				}
			}

			if (!string.IsNullOrEmpty(infoText))
			{
				TimeSpan totalTime = ScheduleHelper.ContractedTime(schedulePart);
				timeText = DateHelper.HourMinutesString(totalTime.TotalMinutes);
			}

			returnList.Add(infoText);
			returnList.Add(periodText);
			returnList.Add(timeText);

			return returnList;
		}
	}
}
