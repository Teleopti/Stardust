﻿using System;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsDetailPresenter : SchedulePresenterBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private IAgentRestrictionsDetailView _view;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IAgentRestrictionsDetailModel _model;

		public AgentRestrictionsDetailPresenter(IAgentRestrictionsDetailView view, IAgentRestrictionsDetailModel model, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
			ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter,IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
			IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
			: base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag)
		{
			_view = view;
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
				//IPreferenceCellData preferenceCellData;
				_model.DetailData().TryGetValue(e.ColIndex - 1, out preferenceCellData);
				if (preferenceCellData == null) e.Style.CellValue = string.Empty;
				else e.Style.CellValue = TeleoptiPrincipal.Current.Regional.Culture.DateTimeFormat.GetDayName(TeleoptiPrincipal.Current.Regional.Culture.Calendar.GetDayOfWeek(preferenceCellData.TheDate));	
				
				return;
			}

			if (e.RowIndex > 0 && e.ColIndex == 0)
			{
				//"--VeckoNum--";
				e.Style.CellType = "RestrictionWeekHeaderViewCellModel";
				e.Style.CellValue = OnQueryWeekHeader(e.RowIndex);
				e.Style.CultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
				return;
			}

			//"--ScheduleDay--";
			if (e.RowIndex < 1 || e.ColIndex < 1) return;
			var currentCell = ((e.RowIndex - 1) * 7) + e.ColIndex;
			_model.DetailData().TryGetValue(currentCell - 1, out preferenceCellData);
			if (preferenceCellData == null) return;

			//e.Style.CellType = "RestrictionSummaryViewCellModel";
			//if (e.Style.CellModel != null) ((IRestrictionSummaryViewCellModel)e.Style.CellModel).RestrictionSummaryPresenter = this;
			//e.Style.CellValue = preferenceCellData.SchedulePart;

			e.Style.CellValue = preferenceCellData.TheDate.ToShortDateString();
			e.Style.Tag = preferenceCellData.TheDate;

			if (preferenceCellData.SchedulePart.FullAccess)
				e.Style.CellTipText = ViewBaseHelper.GetToolTip(preferenceCellData.SchedulePart);

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
		public IWeekHeaderCellData OnQueryWeekHeader(int rowIndex)
		{
			int stop = ((rowIndex - 1) * 7) + 7;
			var minTime = new TimeSpan();
			var maxTime = new TimeSpan();
			IPreferenceCellData preferenceCellData;

			var culture = TeleoptiPrincipal.Current.Regional.Culture;
			var myCal = culture.Calendar;
			var myCwr = culture.DateTimeFormat.CalendarWeekRule;
			var myFirstDow = culture.DateTimeFormat.FirstDayOfWeek;
			var weekNumber = 0;

			var weekMax = new TimeSpan(0);
			for (int index = ((rowIndex - 1) * 7); index < stop; index++)
			{
				if (index >= _model.DetailData().Count)break;
				_model.DetailData().TryGetValue(index, out preferenceCellData);

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

				weekMax = preferenceCellData.WeeklyMax;
				weekNumber = myCal.GetWeekOfYear(preferenceCellData.TheDate, myCwr, myFirstDow);
			}
			var weekIsLegal = minTime <= weekMax;
			IWeekHeaderCellData weekHeaderCell = new WeekHeaderCellData(minTime, maxTime, !weekIsLegal, weekNumber);

			return weekHeaderCell;
		}
	}
}
