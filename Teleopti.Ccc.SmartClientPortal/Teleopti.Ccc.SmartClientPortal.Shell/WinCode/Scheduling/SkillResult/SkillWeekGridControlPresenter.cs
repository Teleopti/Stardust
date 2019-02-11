using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.SkillResult
{
	public interface ISkillWeekGridControl
	{
		void CreateGridRows(ISkill skill, IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder);
		void SetDataSource(ISchedulerStateHolder stateHolder, ISkill skill);
		void SetupGrid(int colCount);
	}

	public class SkillWeekGridControlPresenter
	{
		private readonly ISkillWeekGridControl _view;
		private IList<DateOnlyPeriod> _weeks;
		private IList<DateOnly> _dates;

		public SkillWeekGridControlPresenter(ISkillWeekGridControl view)
		{
			_view = view;
		}

		public void DrawWeekGrid(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			if (stateHolder == null || skill == null) return;
			var timeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
			var dateTimePeriods = stateHolder.RequestedPeriod.Period().WholeDayCollection(timeZone);
			_dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, timeZone))).ToList();

			var weekDates = new List<DateOnly>();
			_weeks = new List<DateOnlyPeriod>();

			foreach (var dateOnly in _dates)
			{
				var weekPeriod = DateHelper.GetWeekPeriod(dateOnly, CultureInfo.CurrentCulture);
				if (!_weeks.Contains(weekPeriod))
				{
					var weekDays = weekPeriod.DayCollection();
					var exists = weekDays.All(weekDateOnly => _dates.Contains(weekDateOnly));
					if (exists) _weeks.Add(weekPeriod);
				}
			}

			foreach (var dateOnlyPeriod in _weeks)
			{
				weekDates.Add(dateOnlyPeriod.StartDate);
			}

			_view.CreateGridRows(skill, weekDates, stateHolder);
			_view.SetDataSource(stateHolder, skill);
			_view.SetupGrid(_weeks.Count);		
		}

		public IList<DateOnlyPeriod> Weeks
		{
			get { return _weeks; }
		}

		public IList<DateOnly> Dates
		{
			get { return _dates; }
		}

		public void SetDates(ISchedulerStateHolder stateHolder)
		{
			if (stateHolder == null) return;
			var timeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
			var dateTimePeriods = stateHolder.RequestedPeriod.Period().WholeDayCollection(timeZone);
			_dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, timeZone))).ToList();	
		}

		public void SetWeeks()
		{
			_weeks = new List<DateOnlyPeriod>();
			foreach (var dateOnly in _dates)
			{
				var weekPeriod = DateHelper.GetWeekPeriod(dateOnly,  CultureInfo.CurrentCulture);
				if (!_weeks.Contains(weekPeriod))
				{
					var weekDays = weekPeriod.DayCollection();
					var exists = weekDays.All(weekDateOnly => _dates.Contains(weekDateOnly));
					if (exists) _weeks.Add(weekPeriod);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>> CreateDataSource(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			if (skill == null || stateHolder == null) return null;

			
			SetDates(stateHolder);
			SetWeeks();
			
			IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>> skillWeekPeriods = new Dictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>();
			IAggregateSkill aggregateSkillSkill = skill;
			var timeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
			foreach (var dateOnlyPeriod in _weeks)
			{
				IList<ISkillStaffPeriod> periods = new List<ISkillStaffPeriod>();
				if (aggregateSkillSkill.IsVirtual)
				{
					periods = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkillSkill, dateOnlyPeriod.ToDateTimePeriod(timeZone));
				}
				else
				{
					var periodToFind = dateOnlyPeriod.ToDateTimePeriod(timeZone);
					periods = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, periodToFind);
				}

				skillWeekPeriods.Add(dateOnlyPeriod, periods);

			}

			return skillWeekPeriods;
		}
	}
}
