using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.SkillResult
{
	public interface ISkillFullPeriodGridControl
	{
		void CreateGridRows(ISkill skill, IList<DateOnly> dates, ISchedulerStateHolder schedulerStateHolder);
		void SetDataSource(ISchedulerStateHolder stateHolder, ISkill skill);
		void SetupGrid(int colCount);
	}

	public class SkillFullPeriodGridControlPresenter
	{
		private readonly ISkillFullPeriodGridControl _view;
		private IList<DateOnlyPeriod> _fullPeriods;
		private IList<DateOnly> _dates;

		public SkillFullPeriodGridControlPresenter(ISkillFullPeriodGridControl view)
		{
			_view = view;
		}

		public void DrawFullPeriodGrid(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			if (stateHolder == null || skill == null) return;

            var dateTimePeriods = stateHolder.RequestedPeriod.Period().WholeDayCollection(TimeZoneGuardForDesktop.Instance.CurrentTimeZone());
			_dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, TimeZoneGuardForDesktop.Instance.CurrentTimeZone()))).OrderBy(s => s.Date).ToList();
			var startDate = _dates.FirstOrDefault();
			var endDate = _dates.LastOrDefault();
			_fullPeriods = new List<DateOnlyPeriod> {new DateOnlyPeriod(startDate, endDate)};

			_view.CreateGridRows(skill, _dates, stateHolder);
			_view.SetDataSource(stateHolder, skill);
			_view.SetupGrid(_fullPeriods.Count);
		}

		public IList<DateOnlyPeriod> FullPeriods
		{
			get { return _fullPeriods; }
		}

		public IList<DateOnly> Dates
		{
			get { return _dates; }
		}

		public void SetDates(ISchedulerStateHolder stateHolder)
		{
			if (stateHolder == null) return;
			var timeZone = TimeZoneGuardForDesktop.Instance.CurrentTimeZone();
			var dateTimePeriods = stateHolder.RequestedPeriod.Period().WholeDayCollection(timeZone);
			_dates = dateTimePeriods.Select(d => new DateOnly(TimeZoneHelper.ConvertFromUtc(d.StartDateTime, timeZone))).OrderBy(s => s.Date).ToList();
		}

		public void SetFullPeriods()
		{
			_fullPeriods = new List<DateOnlyPeriod>();
			var startDate = _dates.FirstOrDefault();
			var endDate = _dates.LastOrDefault();
			_fullPeriods.Add(new DateOnlyPeriod(startDate, endDate));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>> CreateDataSource(ISchedulerStateHolder stateHolder, ISkill skill)
		{
			if (skill == null || stateHolder == null) return null;

			
			SetDates(stateHolder);
			SetFullPeriods();
			
			IDictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>> skillFullPeriods = new Dictionary<DateOnlyPeriod, IList<ISkillStaffPeriod>>();
			IAggregateSkill aggregateSkillSkill = skill;
			var timeZone = TimeZoneGuardForDesktop.Instance.CurrentTimeZone();
			foreach (var dateOnlyPeriod in _fullPeriods)
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

				skillFullPeriods.Add(dateOnlyPeriod, periods);

			}

			return skillFullPeriods;
		}
	}
}
