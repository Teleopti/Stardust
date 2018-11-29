using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.SkillResult
{
	public interface ISkillMonthGridControl
	{
		void CreateGridRows(ISkill skill, IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder);
		void SetDataSource(ISchedulerStateHolder stateHolder, ISkill skill);
		void SetupGrid(int colCount);
	}

	public class SkillMonthGridControlPresenter
	{
		private readonly ISkillMonthGridControl _view;
		private IList<DateOnlyPeriod> _months;
		private IList<DateOnly> _dates;

		public SkillMonthGridControlPresenter(ISkillMonthGridControl view)
		{
			_view = view;
		}

		public void DrawMonthGrid(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			if (stateHolder == null || skill == null) return;

			var dateTimePeriods = stateHolder.RequestedPeriod.Period().WholeDayCollection(stateHolder.TimeZoneInfo );
			_dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, stateHolder.TimeZoneInfo))).ToList();

			var monthDates = new List<DateOnly>();
			_months = new List<DateOnlyPeriod>();

			foreach (var dateOnly in _dates)
			{
				var startDate = DateHelper.GetFirstDateInMonth(dateOnly.Date, CultureInfo.CurrentCulture);
				var endDate = DateHelper.GetLastDateInMonth(dateOnly.Date, CultureInfo.CurrentCulture);
				var monthPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

				if (!_months.Contains(monthPeriod))
				{
					var monthDays = monthPeriod.DayCollection();
					var exists = monthDays.All(monthDateOnly => _dates.Contains(monthDateOnly));
					if (exists) _months.Add(monthPeriod);
				}
			}

			foreach (var dateOnlyPeriod in _months)
			{
				monthDates.Add(dateOnlyPeriod.StartDate);
			}

			_view.CreateGridRows(skill, monthDates, stateHolder);
			_view.SetDataSource(stateHolder, skill);
			_view.SetupGrid(_months.Count);
		}

		public IList<DateOnlyPeriod> Months
		{
			get { return _months; }
		}

		public IList<DateOnly> Dates
		{
			get { return _dates; }
		}

		public void SetDates(ISchedulerStateHolder stateHolder)
		{
			if (stateHolder == null) return;

			var dateTimePeriods = stateHolder.RequestedPeriod.Period().WholeDayCollection(stateHolder.TimeZoneInfo );
			_dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, stateHolder.TimeZoneInfo))).ToList();	
		}

		public void SetMonths()
		{
			_months = new List<DateOnlyPeriod>();
			foreach (var dateOnly in _dates)
			{
				var startDate = DateHelper.GetFirstDateInMonth(dateOnly.Date, CultureInfo.CurrentCulture);
				var endDate = DateHelper.GetLastDateInMonth(dateOnly.Date, CultureInfo.CurrentCulture);
				var monthPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

				if (!_months.Contains(monthPeriod))
				{
					var monthDays = monthPeriod.DayCollection();
					var exists = monthDays.All(monthDateOnly => _dates.Contains(monthDateOnly));
					if (exists) _months.Add(monthPeriod);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>> CreateDataSource(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			if (skill == null || stateHolder == null) return null;

			
			SetDates(stateHolder);
			SetMonths();
			
			IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>> skillMonthPeriods = new Dictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>();
			IAggregateSkill aggregateSkillSkill = skill;

			foreach (var dateOnlyPeriod in _months)
			{
				IList<ISkillStaffPeriod> periods = new List<ISkillStaffPeriod>();
				if (aggregateSkillSkill.IsVirtual)
				{
					periods = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateOnlyPeriod.ToDateTimePeriod(stateHolder.TimeZoneInfo));
				}
				else
				{
					var periodToFind = dateOnlyPeriod.ToDateTimePeriod(stateHolder.TimeZoneInfo);
					periods = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, periodToFind);
				}

				skillMonthPeriods.Add(dateOnlyPeriod, periods);

			}

			return skillMonthPeriods;
		}
	}
}
