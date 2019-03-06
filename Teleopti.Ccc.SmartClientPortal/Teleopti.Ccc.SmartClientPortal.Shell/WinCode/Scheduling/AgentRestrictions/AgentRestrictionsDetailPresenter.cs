using System;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsDetailPresenter : SchedulePresenterBase
	{
		private readonly IAgentRestrictionsDetailModel _model;

		public AgentRestrictionsDetailPresenter(IAgentRestrictionsDetailView view, IAgentRestrictionsDetailModel model, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
			ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter,IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
			IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
			: base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer, timeZoneGuard)
		{
			_model = model;
		}

		public override int RowCount
		{
			get { return (_model.DetailData().Count + 7 - 1) / 7; }
		}

		public override int ColCount
		{
			get { return 7; }
		}

		public override void QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
		{
			if (e == null) return;
			if (e.RowIndex < 0 || e.ColIndex < 0) return;
			if (e.RowIndex == 0 && e.ColIndex == 0) return;

			IPreferenceCellData preferenceCellData;

			if (e.RowIndex == 0 && e.ColIndex > 0)
			{
				//"--Veckodag--";
				_model.DetailData().TryGetValue(e.ColIndex - 1, out preferenceCellData);
				if (preferenceCellData == null) e.Style.CellValue = string.Empty;
				else e.Style.CellValue = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture.DateTimeFormat.GetDayName(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture.Calendar.GetDayOfWeek(preferenceCellData.TheDate.Date));	
				
				return;
			}

			if (e.RowIndex > 0 && e.ColIndex == 0)
			{
				//"--VeckoNum--";
				e.Style.CellType = "RestrictionWeekHeaderViewCellModel";
				e.Style.CellValue = OnQueryWeekHeader(e.RowIndex);
				e.Style.CultureInfo = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
				return;
			}

			//"--ScheduleDay--";
			if (e.RowIndex < 1 || e.ColIndex < 1) return;
			var currentCell = ((e.RowIndex - 1) * 7) + e.ColIndex;
			_model.DetailData().TryGetValue(currentCell - 1, out preferenceCellData);
			if (preferenceCellData == null) return;

			e.Style.CellType = "AgentRestrictionsDetailViewCellModel";
			if (e.Style.CellModel != null) ((IAgentRestrictionsDetailViewCellModel)e.Style.CellModel).DetailModel = _model;
			e.Style.CellValue = preferenceCellData.SchedulePart;
			e.Style.Tag = preferenceCellData.TheDate;

			if (preferenceCellData.SchedulePart.FullAccess)
				e.Style.CellTipText = ViewBaseHelper.GetToolTip(preferenceCellData.SchedulePart, TimeZoneGuard);

			if (preferenceCellData.ViolatesNightlyRest)
			{
				var sb = new StringBuilder(e.Style.CellTipText);
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(Resources.RestrictionViolatesNightRest);
				e.Style.CellTipText = sb.ToString();
			}

			if (preferenceCellData.NoShiftsCanBeFound)
			{
				var sb = new StringBuilder(e.Style.CellTipText);
				if (sb.Length > 0) sb.AppendLine();
				sb.Append(Resources.NoShiftsFound);
				e.Style.CellTipText = sb.ToString();
			}
		}
		
		public IWeekHeaderCellData OnQueryWeekHeader(int rowIndex)
		{
			int stop = ((rowIndex - 1) * 7) + 7;
			var minTime = TimeSpan.Zero;
			var maxTime = TimeSpan.Zero;

			var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			var weekNumber = 0;

			//var weekMax = new TimeSpan(0);
		    var weekMax = TimeSpan.MinValue;
		    var weekMin = TimeSpan.MaxValue;
		    var haveFullTimePersonPeriod = true;

			var startIndex = (rowIndex - 1) * 7;
			for (int index = startIndex; index < stop; index++)
			{
				if (index >= _model.DetailData().Count)break;
				IPreferenceCellData preferenceCellData;
				_model.DetailData().TryGetValue(index, out preferenceCellData);
			   
				var projection = preferenceCellData.SchedulePart.ProjectionService().CreateProjection();
				if (preferenceCellData.SchedulingOption.UseScheduling &&
				    projection.HasLayers)
				{
					var workTime = projection.WorkTime();
				    minTime = minTime.Add(workTime);
				    maxTime = maxTime.Add(workTime);
				}
				
				else
					if (preferenceCellData.EffectiveRestriction != null)
					{
						if (preferenceCellData.EffectiveRestriction.WorkTimeLimitation.HasValue())
						{
							if (preferenceCellData.EffectiveRestriction.WorkTimeLimitation.StartTime.HasValue)
								minTime = minTime.Add(preferenceCellData.EffectiveRestriction.WorkTimeLimitation.StartTime.Value);
							if (preferenceCellData.EffectiveRestriction.WorkTimeLimitation.EndTime.HasValue)
								maxTime = maxTime.Add(preferenceCellData.EffectiveRestriction.WorkTimeLimitation.EndTime.Value);
						}
					}

			    if (preferenceCellData.WeeklyMax > weekMax) weekMax = preferenceCellData.WeeklyMax;
			    if (preferenceCellData.WeeklyMin < weekMin) weekMin = preferenceCellData.WeeklyMin;
                if (preferenceCellData.EmploymentType.Equals(EmploymentType.HourlyStaff)) haveFullTimePersonPeriod = false;
                
				weekNumber = DateHelper.WeekNumber(preferenceCellData.TheDate.Date, culture);
			}
			var weekIsLegal = minTime <= weekMax;
            if (weekIsLegal && haveFullTimePersonPeriod) weekIsLegal = maxTime >= weekMin;
			IWeekHeaderCellData weekHeaderCell = new WeekHeaderCellData(minTime, maxTime, !weekIsLegal, weekNumber);

			return weekHeaderCell;
		}
	}
}
