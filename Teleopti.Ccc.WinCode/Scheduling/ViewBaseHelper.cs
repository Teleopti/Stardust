using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Scheduling.Panels;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// ViewBase helper
    /// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public static class ViewBaseHelper
    {
        /// <summary>
        /// GetToolTip
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string GetToolTip(IScheduleDay cell)
        {
            StringBuilder sb = new StringBuilder();

            string conflictingAssignments = GetToolTipConflictingAssignments(cell);
            string assignments = GetToolTipAssignments(cell);
            string absences = GetToolTipAbsences(cell);
            string dayOff = GetToolTipDayOff(cell);
            string businessRuleConflicts = GetToolTipBusinessRuleConflicts(cell);
            string meetings = GetToolTipMeetings(cell);
            string overtime = GetToolTipOvertime(cell);
            

            if (conflictingAssignments.Length > 0)
                sb.Append(conflictingAssignments);

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
        public static string GetToolTipBusinessRuleConflicts(ISchedulePart cell)
        {
            StringBuilder sb = new StringBuilder();
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
        /// <summary>
        /// Get tooltip for conflicting assignments
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string GetToolTipConflictingAssignments(ISchedulePart cell)
        {
            StringBuilder sb = new StringBuilder();

            IList<IPersonAssignment> conflicts = cell.PersonAssignmentConflictCollection;
            if (conflicts.Count > 0)
            {
                foreach (IPersonAssignment pa in conflicts)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    if(pa.MainShift != null)
                        sb.Append(pa.MainShift.ShiftCategory.Description.Name);             //name
                    sb.Append("  ");
                    sb.Append(ToLocalStartEndTimeString(pa.Period, cell.TimeZone));      //time
                }
            }

            if (sb.Length > 0)
                return string.Format(CultureInfo.CurrentUICulture, "{0}{1}({2})",UserTexts.Resources.OverlappningShifts, Environment.NewLine, sb);
            return string.Empty;
        }

        public static string ToLocalStartEndTimeString(DateTimePeriod period, TimeZoneInfo timeZoneInfo)
        {
			var culture = TeleoptiPrincipal.Current.Regional.Culture;
            const string separator = " - ";

            return string.Concat(TimeHelper.TimeOfDayFromTimeSpan(period.StartDateTimeLocal(timeZoneInfo).TimeOfDay, culture), separator,
                                 TimeHelper.TimeOfDayFromTimeSpan(period.EndDateTimeLocal(timeZoneInfo).TimeOfDay, culture));
        }

        public static string ToLocalStartEndTimeStringAbsences(DateTimePeriod partPeriod, DateTimePeriod absencePeriod, TimeZoneInfo timeZoneInfo)
        {
			var culture = TeleoptiPrincipal.Current.Regional.Culture;
            const string separator = " - ";
            DateTimePeriod startTimePeriod = absencePeriod;
            DateTimePeriod endTimePeriod = absencePeriod;

            if (absencePeriod.StartDateTime < partPeriod.StartDateTime)
                startTimePeriod = partPeriod;

            if (absencePeriod.EndDateTime > partPeriod.EndDateTime)
                endTimePeriod = partPeriod;

            return string.Concat(TimeHelper.TimeOfDayFromTimeSpan(startTimePeriod.StartDateTimeLocal(timeZoneInfo).TimeOfDay, culture), separator,
									TimeHelper.TimeOfDayFromTimeSpan(endTimePeriod.EndDateTimeLocal(timeZoneInfo).TimeOfDay, culture));
        }

        /// <summary>
        /// Get tooltip for assignments
        /// </summary>
        /// <param name="scheduleDay"></param>
        /// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static string GetToolTipAssignments(IScheduleDay scheduleDay)
        {
            StringBuilder sb = new StringBuilder();

            IList<IPersonAssignment> asses = scheduleDay.PersonAssignmentCollection();
            if (asses.Count > 0)
            {
                foreach (IPersonAssignment pa in asses)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    if(pa.MainShift != null)
                        sb.Append(pa.MainShift.ShiftCategory.Description.Name);             //name
                    sb.Append("  ");
                    sb.Append(ToLocalStartEndTimeString(pa.Period,scheduleDay.TimeZone));      //time

                    foreach(PersonalShift ps in pa.PersonalShiftCollection) 
                    {
                        sb.AppendLine();
                        sb.AppendFormat(" - {0}: ", UserTexts.Resources.PersonalShift);
                        foreach (ActivityLayer layer in ps.LayerCollection)
                        {
                            sb.AppendLine();
                            sb.Append("    ");
                            sb.Append(layer.Payload.ConfidentialDescription(pa.Person,scheduleDay.DateOnlyAsPeriod.DateOnly).Name);                                  //name
                            sb.Append(": ");
                            sb.Append(ToLocalStartEndTimeString(layer.Period, scheduleDay.TimeZone));             //time
                        }
                    }
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
            StringBuilder sb = new StringBuilder();

            IList<IPersonAbsence> abses = cell.PersonAbsenceCollection();
            if (abses.Count > 0)
            {
                foreach (IPersonAbsence pa in abses)
                {
                    if (sb.Length > 0) sb.AppendLine();

                    sb.Append(pa.Layer.Payload.ConfidentialDescription(pa.Person,cell.DateOnlyAsPeriod.DateOnly).Name); //name
                    sb.Append(": ");
                    sb.Append(ToLocalStartEndTimeStringAbsences(cell.Period, pa.Layer.Period, cell.TimeZone));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get tooltip for meetings
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string GetToolTipMeetings(ISchedulePart cell)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IPersonMeeting personMeeting in cell.PersonMeetingCollection())
            {
                if (sb.Length > 0) sb.AppendLine();

                sb.Append(personMeeting.BelongsToMeeting.GetSubject(new NoFormatting()));
                sb.Append(": ");
            	sb.Append(ToLocalStartEndTimeString(personMeeting.Period, cell.TimeZone));

                if (personMeeting.Optional)
                    sb.AppendFormat(" ({0})", UserTexts.Resources.Optional);
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
            StringBuilder sb = new StringBuilder();

            var proj = cell.ProjectionService().CreateProjection();

            foreach (IVisualLayer layer in proj)
            {
                if (layer.DefinitionSet == null)
                    continue;

                if (sb.Length > 0) sb.AppendLine();
                sb.Append(layer.DefinitionSet.Name);
                sb.Append(": ");
                sb.Append(layer.Payload.ConfidentialDescription(cell.Person,cell.DateOnlyAsPeriod.DateOnly).Name);
                sb.Append(": ");
                sb.Append(ToLocalStartEndTimeString(layer.Period, cell.TimeZone));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get tooltip for absences
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string GetToolTipDayOff(ISchedulePart cell)
        {
			StringBuilder sb = new StringBuilder();
        	var culture = TeleoptiPrincipal.Current.Regional.Culture;

            var personDayOffs = cell.PersonDayOffCollection();
            if (personDayOffs.Count > 0)
            {
            	var dayOff = personDayOffs[0].DayOff;
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

			return sb.ToString();
        }

        /// <summary>
        /// calculate widht for minutes
        /// </summary>
        /// <param name="min"></param>
        /// <param name="hourWidth"></param>
        /// <returns></returns>
        public static int GetMinWidth(int min, int hourWidth)
        {
            float minPercent = min / (float)60;
            float minWidth = minPercent * hourWidth;

            return (int)minWidth;
        }

        /// <summary>
        /// Return a rectangle to draw from in a timeline view
        /// </summary>
        /// <param name="periodBounds">The period bounds.</param>
        /// <param name="destinationRectangle">The destination rectangle.</param>
        /// <param name="period">The period.</param>
        /// <param name="isRightToLeft">if set to <c>true</c> [is right to left].</param>
        /// <returns></returns>
        public static Rectangle GetLayerRectangle(DateTimePeriod periodBounds, Rectangle destinationRectangle, DateTimePeriod period, bool isRightToLeft)
        {
            DateTimePeriod? periodIntersection = period.Intersection(periodBounds);
            if (!periodIntersection.HasValue) return new Rectangle();

            LengthToTimeCalculator calculator = new LengthToTimeCalculator(periodBounds, destinationRectangle.Width);
            Rectangle rect = calculator.RectangleFromDateTimePeriod(periodIntersection.Value, new Point(destinationRectangle.X, destinationRectangle.Y + 2), destinationRectangle.Height - 4, isRightToLeft);

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
            Dictionary<int, DateOnly> firstDateOfWeek = new Dictionary<int, DateOnly>();
            
            foreach (var day in selectedPeriod.DayCollection())
            {
                var weekNumber = DateHelper.WeekNumber(day, CultureInfo.CurrentCulture);
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
            
            return new DateOnlyPeriod(start.Value,end.Value);

        }

        #region Style current

        public static void StyleCurrentContractTimeCell(GridStyleInfo style, IScheduleRange wholeRange, DateOnlyPeriod period)
        {
            style.CellType = "TotalTimeCell";
            if (!wholeRange.CalculatedContractTimeHolder.HasValue)
            {
                TimeSpan contractTime = TimeSpan.Zero;
                foreach (var scheduleDay in wholeRange.ScheduledDayCollection(period))
                {
                    DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
                    IPerson person = scheduleDay.Person;
                    if (person.Period(dateOnly) == null)
                        continue;
                    if (person.TerminalDate != null && scheduleDay.Person.TerminalDate < dateOnly)
                        continue;

                    contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
                }
                wholeRange.CalculatedContractTimeHolder = contractTime;
            }
            style.CellValue = wholeRange.CalculatedContractTimeHolder.Value;
        }

		public static TimeSpan CurrentContractTime(IScheduleRange wholeRange, DateOnlyPeriod period)
		{
			if(wholeRange == null)
				throw new ArgumentNullException("wholeRange");

			TimeSpan contractTime = TimeSpan.Zero;

			if (!wholeRange.CalculatedContractTimeHolder.HasValue)
			{
				
				foreach (var scheduleDay in wholeRange.ScheduledDayCollection(period))
				{
					contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
				}

				return contractTime;
			}

			return wholeRange.CalculatedContractTimeHolder.Value;
		}

		public static Boolean CheckOpenPeriodMatchSchedulePeriod(IPerson person, DateOnlyPeriod openPeriod)
		{
			if(person == null)
				throw new ArgumentNullException("person");

			var schedulePeriods = person.PersonSchedulePeriods(openPeriod);
			if (!schedulePeriods.Any()) return false;

			var startPeriod = schedulePeriods[0].GetSchedulePeriod(openPeriod.StartDate);
			var endPeriod = schedulePeriods[schedulePeriods.Count - 1].GetSchedulePeriod(openPeriod.EndDate);
			if (startPeriod == null || endPeriod == null) return false;
			return startPeriod.Value.StartDate == openPeriod.StartDate && endPeriod.Value.EndDate == openPeriod.EndDate;
		}

        public static void StyleCurrentTotalDayOffCell(GridStyleInfo style, IScheduleRange wholeRange, DateOnlyPeriod period)
        {
            style.CellType = "TotalDayOffCell";

			//if (!wholeRange.CalculatedScheduleDaysOff.HasValue)
			//{
			//    int totalDayOff = 0;
			//    foreach (var scheduleDay in wholeRange.ScheduledDayCollection(period))
			//    {
			//        SchedulePartView significant = scheduleDay.SignificantPart();
			//        if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
			//            totalDayOff += 1;
			//    }
			//    wholeRange.CalculatedScheduleDaysOff = totalDayOff;
	
			wholeRange.CalculatedScheduleDaysOff = CurrentTotalDayOffs(wholeRange, period);
			//}
            style.CellValue = wholeRange.CalculatedScheduleDaysOff.Value;
        }

		public static int CurrentTotalDayOffs(IScheduleRange wholeRange, DateOnlyPeriod period)
		{
			if(wholeRange == null)
				throw new ArgumentNullException("wholeRange");

			var totalDayOff = 0;

			if (!wholeRange.CalculatedScheduleDaysOff.HasValue)
			{
				foreach (var scheduleDay in wholeRange.ScheduledDayCollection(period))
				{
				    DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
				    IPerson person = scheduleDay.Person;
                    if (person.Period(dateOnly) == null)
                        continue;
                    if (person.TerminalDate != null && scheduleDay.Person.TerminalDate < dateOnly)
                        continue;

					SchedulePartView significant = scheduleDay.SignificantPart();
					if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
						totalDayOff += 1;
				}

				return totalDayOff;
			}

			return wholeRange.CalculatedScheduleDaysOff.Value;	
		}

        #endregion

        #region Style target

		public static void StyleTargetScheduleContractTimeCell(GridStyleInfo style, IPerson person, DateOnlyPeriod openPeriod, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleRange wholeRange)
		{

			if (style == null)
				throw new ArgumentNullException("style");

			if (wholeRange == null)
				throw new ArgumentNullException("wholeRange");

			if (!CheckOpenPeriodMatchSchedulePeriod(person, openPeriod))
			{
				SetCellNotApplicable(style);
				return;
			}

			if (!CheckOverrideTargetTimeLoadedAndScheduledPeriod(person, openPeriod))
			{
				SetCellNotApplicable(style);
				return;
			}

			style.CellType = "TotalTimeCell";
			if (!wholeRange.CalculatedTargetTimeHolder.HasValue)
			{
				HashSet<IVirtualSchedulePeriod> virtualSchedulePeriods = ExtractVirtualPeriods(person, openPeriod);
				TimeSpan? targetTime = CalculateTargetTime(virtualSchedulePeriods, schedulingResultStateHolder, true);
				if(!targetTime.HasValue)
				{
					SetCellNotApplicable(style);
					return;
				}
				wholeRange.CalculatedTargetTimeHolder = targetTime.Value;
			}

			style.CellValue = wholeRange.CalculatedTargetTimeHolder;


		}

    	public static void StyleTargetScheduleDaysOffCell(GridStyleInfo style, IPerson person, DateOnlyPeriod openPeriod, IScheduleRange wholeRange)
        {
			if(style == null)
				throw new ArgumentNullException("style");

			if(person == null)
				throw new ArgumentNullException("person");

			if (!CheckOpenPeriodMatchSchedulePeriod(person, openPeriod))
			{
				SetCellNotApplicable(style);
				return;
			}

			if (!CheckOverrideDayOffAndLoadedAndScheduledPeriod(person, openPeriod))
			{
				SetCellNotApplicable(style);
				return;
			}

				style.CellType = "TotalDayOffCell";
				if(!wholeRange.CalculatedTargetScheduleDaysOff.HasValue)
				{
					HashSet<IVirtualSchedulePeriod> virtualSchedulePeriods = ExtractVirtualPeriods(person, openPeriod);
					wholeRange.CalculatedTargetScheduleDaysOff = CalculateTargetDaysOff(virtualSchedulePeriods);
				}
					

				style.CellValue = wholeRange.CalculatedTargetScheduleDaysOff;
 
        }

        private static void SetCellNotApplicable(GridStyleInfo style) 
        {
            style.CellType = "Static";
            style.CellValue = UserTexts.Resources.NA;
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

		public static HashSet<IVirtualSchedulePeriod> ExtractVirtualPeriods(IPerson person, DateOnlyPeriod period)
		{
			if(person == null)
				throw new ArgumentNullException("person");

			HashSet<IVirtualSchedulePeriod> virtualPeriods = new HashSet<IVirtualSchedulePeriod>();
			foreach (var dateOnly in period.DayCollection())
			{
				virtualPeriods.Add(person.VirtualSchedulePeriod(dateOnly));
			}
			return virtualPeriods;
		}

		public static int CalculateTargetDaysOff(HashSet<IVirtualSchedulePeriod> extractVirtualPeriods)
		{
			if(extractVirtualPeriods == null)
				throw new ArgumentNullException("extractVirtualPeriods");

			var ret = 0;

			foreach (var virtualSchedulePeriod in extractVirtualPeriods)
			{
				ret += virtualSchedulePeriod.DaysOff();
			}

			return ret;
		}

		public static TimeSpan? CalculateTargetTime(HashSet<IVirtualSchedulePeriod> extractVirtualPeriods, ISchedulingResultStateHolder schedulingResultStateHolder, bool seasonality)
		{
			if(extractVirtualPeriods == null)
				throw new ArgumentNullException("extractVirtualPeriods");

			TimeSpan ret = TimeSpan.Zero;
			foreach (var virtualSchedulePeriod in extractVirtualPeriods)
			{
				IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
						new FullWeekOuterWeekPeriodCreator(virtualSchedulePeriod.DateOnlyPeriod, virtualSchedulePeriod.Person);
				IScheduleMatrixPro matrix = new ScheduleMatrixPro(schedulingResultStateHolder, fullWeekOuterWeekPeriodCreator, virtualSchedulePeriod);
				ISchedulePeriodTargetCalculatorFactory schedulePeriodTargetCalculatorFactory =
					new NewSchedulePeriodTargetCalculatorFactory(matrix);
				ISchedulePeriodTargetCalculator calculator = schedulePeriodTargetCalculatorFactory.CreatePeriodTargetCalculator();
				if (calculator == null)
					return null;

				ret = ret.Add(calculator.PeriodTarget(seasonality));
			}
			return ret;
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
        /// Gets displaymode for assignment
        /// </summary>
        /// <param name="pa">The pa.</param>
        /// <param name="part">The part.</param>
        /// <param name="layerCollection">The layer collection.</param>
        /// <returns></returns>
        public static DisplayMode GetAbsenceDisplayMode(IPersonAbsence pa, ISchedulePart part, IVisualLayerCollection layerCollection)
        {
            DateTimePeriod period = pa.Layer.Period;
            DateTimePeriod datePeriod = part.Period;
            if (period.ContainsPart(datePeriod))
            {
                if (layerCollection.IsSatisfiedBy(VisualLayerCollectionSpecification.OneAbsenceLayer))
                    return DisplayMode.WholeDay;

                DateTime layerStartDateTime = period.StartDateTimeLocal(part.TimeZone);
                DateTime layerEndDateTime = period.EndDateTimeLocal(part.TimeZone);
                DateTime periodStartDateTime = datePeriod.StartDateTimeLocal(part.TimeZone);
                DateTime periodEndDateTime = datePeriod.EndDateTimeLocal(part.TimeZone);

                if (layerStartDateTime >= periodStartDateTime && layerEndDateTime<= periodEndDateTime)
                    return DisplayMode.BeginsAndEndsToday;

                if (layerStartDateTime < periodStartDateTime && layerEndDateTime <= periodEndDateTime)
                    return DisplayMode.EndsToday;

                if (layerStartDateTime >= periodStartDateTime && layerEndDateTime > periodEndDateTime)
                    return DisplayMode.BeginsToday;
            }

            return DisplayMode.WholeDay;
        }

        /// <summary>
        /// Gets displaymode for absence
        /// </summary>
        /// <param name="pa">The pa.</param>
        /// <param name="part">The part.</param>
        /// <returns></returns>
        public static DisplayMode GetAssignmentDisplayMode(IPeriodized pa, ISchedulePart part)
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

            IPersonAssignment pa = schedulePart.AssignmentHighZOrder();

            if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
            {
                IVisualLayerCollection layerCollection = schedulePart.ProjectionService().CreateProjection();
                foreach (IVisualLayer layer in layerCollection)
                {
                    infoText = layer.DisplayDescription().Name;
                    break;
                }
                if (layerCollection.Count() == 0)
                {
                    //we have no underlaying activity = is on top of day off
                    var absenceCollection = schedulePart.PersonAbsenceCollection();
                    IPersonAbsence personAbsence = absenceCollection[absenceCollection.Count - 1];
                    infoText = personAbsence.Layer.Payload.Description.Name;
                }
                periodText = "-";
            }

            if (significantPart == SchedulePartView.DayOff)
            {
                infoText = schedulePart.PersonDayOffCollection()[0].DayOff.Description.Name;
                periodText = "-";
            }

            if (significantPart == SchedulePartView.MainShift)
            {
                infoText = pa.MainShift.ShiftCategory.Description.Name;
                periodText = ToLocalStartEndTimeString(pa.Period, schedulePart.TimeZone);
            }

            if(!string.IsNullOrEmpty(infoText))
            {
                TimeSpan totalTime = ScheduleHelper.ContractedTime(schedulePart);
                timeText = DateHelper.HourMinutesString(totalTime.TotalMinutes);
            }

            returnList.Add(infoText);
            returnList.Add(periodText);
            returnList.Add(timeText);

            return returnList;
        }

		public static DateOnlyPeriod? PeriodFromSchedulePeriods(IEnumerable<IPerson> persons, DateOnlyPeriod period)
		{
			var min = DateOnly.MaxValue;
			var max = DateOnly.MinValue;

			foreach (var schedulePeriod in persons.Select(person => person.PhysicalSchedulePeriods(period)).SelectMany(schedulePeriods => schedulePeriods))
			{
				if (schedulePeriod.DateFrom.Date < min)
					min = schedulePeriod.DateFrom;

				if (schedulePeriod.RealDateTo().Date > max)
					max = schedulePeriod.RealDateTo();
			}

			if (min > max) return null;

			return new DateOnlyPeriod(min, max);
		}
    }
}
