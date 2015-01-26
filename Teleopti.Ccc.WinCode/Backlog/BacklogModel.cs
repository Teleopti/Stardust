using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Backlog
{
	public class BacklogModel
	{
		private ISchedulerStateHolder _stateHolder = new SchedulerStateHolder(new SchedulingResultStateHolder());
		private IDictionary<ISkill, IDictionary<DateOnly, BacklogTask>> _taskDic = new Dictionary<ISkill, IDictionary<DateOnly, BacklogTask>>();
		private BacklogTaskForecastedTimePerDateCalculator _backlogTaskForecastedTimePerDateCalculator = new BacklogTaskForecastedTimePerDateCalculator();
		private IDictionary<ISkill, IDictionary<DateOnly, TimeSpan>> _manualEntries = new Dictionary<ISkill, IDictionary<DateOnly, TimeSpan>>();

		public void Load()
		{
			var period = new DateOnlyPeriod(new DateOnly(2011, 05, 02), new DateOnly(2011, 05, 29));
			var dateOnlyPeriodAsDateTimePeriod = new DateOnlyPeriodAsDateTimePeriod(period, TimeZoneGuard.Instance.TimeZone);
			_stateHolder.RequestedPeriod = dateOnlyPeriodAsDateTimePeriod;
			
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var scenarioRepository = new ScenarioRepository(uow);
				loadCommonStateHolder(uow,_stateHolder);				
				loadSkills(uow, _stateHolder);
				var skills = _stateHolder.SchedulingResultState.Skills.ToList();
				foreach (var skill in skills)
				{
					if (!(skill.SkillType.ForecastSource == ForecastSource.Email || skill.SkillType.ForecastSource == ForecastSource.Backoffice))
						_stateHolder.SchedulingResultState.Skills.Remove(skill);
				}
				_stateHolder.SetRequestedScenario(scenarioRepository.LoadDefaultScenario());
				loadSkillDays(uow, _stateHolder, dateOnlyPeriodAsDateTimePeriod.Period());
			}
			createBacklogTasks(period);
			setClosedDates(period);
		}

		public DateOnlyPeriod Period()
		{
			return _stateHolder.RequestedPeriod.DateOnlyPeriod;
		}

		public IEnumerable<ISkill> GetTabSkillList()
		{
			return _stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Email).ToList();
		}

		private void createBacklogTasks(DateOnlyPeriod period)
		{			
			foreach (var skill in _stateHolder.SchedulingResultState.Skills)
			{
				if (skill.SkillType.ForecastSource == ForecastSource.Backoffice)
					continue;

				if(!_taskDic.ContainsKey(skill))
					_taskDic.Add(skill, new Dictionary<DateOnly, BacklogTask>());
			}
			foreach (var date in period.DayCollection())
			{
				foreach (var skillDay in _stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new List<DateOnly>{date}))
				{
					if (!_taskDic.ContainsKey(skillDay.Skill))
						continue;

					TimeSpan serviceLevel;
					TimeSpan.TryParse(skillDay.Skill.Description, out serviceLevel);
					if (serviceLevel < TimeSpan.FromDays(1))
						serviceLevel = TimeSpan.FromDays(1);
					var task = new BacklogTask(skillDay.ForecastedIncomingDemand, skillDay.CurrentDate, serviceLevel);
					_taskDic[skillDay.Skill].Add(date, task);
				}
			}
		}

		private void setClosedDates(DateOnlyPeriod period)
		{
			foreach (var date in period.DayCollection())
			{
				foreach (var skillDay in _stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new List<DateOnly> {date}))
				{
					if (skillDay.OpenForWork.IsOpen)
						continue;

					if(!_taskDic.ContainsKey(skillDay.Skill))
						continue;

					foreach (var task in _taskDic[skillDay.Skill].Values)
					{
						task.CloseDate(date);
					}
				}
			}
		}

		private static void loadCommonStateHolder(IUnitOfWork uow, ISchedulerStateHolder stateHolder)
		{
			stateHolder.LoadCommonState(uow, new RepositoryFactory());
			if (!stateHolder.CommonStateHolder.DayOffs.Any())
				throw new StateHolderException("You must create at least one Day Off in Options!");
		}

		private void loadSkills(IUnitOfWork uow, ISchedulerStateHolder stateHolder)
		{
			ICollection<ISkill> skills = new SkillRepository(uow).FindAllWithSkillDays(stateHolder.RequestedPeriod.DateOnlyPeriod);
			foreach (ISkill skill in skills)
			{
				if (skill.SkillType is SkillTypePhone)
					skill.SkillType.StaffingCalculatorService = new StaffingCalculatorServiceFacade(true);
				stateHolder.SchedulingResultState.Skills.Add(skill);
			}
		}

		private void loadSkillDays(IUnitOfWork uow, ISchedulerStateHolder stateHolder, DateTimePeriod periodToLoad)
		{
			using (PerformanceOutput.ForOperation("Loading skill days"))
			{
				ISkillDayRepository skillDayRepository = new SkillDayRepository(uow);
				IMultisiteDayRepository multisiteDayRepository = new MultisiteDayRepository(uow);
				stateHolder.SchedulingResultState.SkillDays = new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository).
					LoadSchedulerSkillDays(
						new DateOnlyPeriod(stateHolder.RequestedPeriod.DateOnlyPeriod.StartDate.AddDays(-8),
							stateHolder.RequestedPeriod.DateOnlyPeriod.EndDate.AddDays(8)), stateHolder.SchedulingResultState.Skills,
						stateHolder.RequestedScenario);

				IList<ISkillStaffPeriod> skillStaffPeriods =
					stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodList(
						stateHolder.SchedulingResultState.Skills, periodToLoad);

				foreach (ISkillStaffPeriod period in skillStaffPeriods)
				{
					period.Payload.UseShrinkage = false;
				}
			}
		}

		public string GetDateForIndex(int colIndex)
		{
			var cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			return _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex-1].ToShortDateString(cultureInfo);
		}

		public TimeSpan GetIncomingForIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex-1];
			return _taskDic[skill][date].TotalIncoming();
		}

		public TimeSpan GetForecastedForIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex-1];
			return _backlogTaskForecastedTimePerDateCalculator.CalculateForDate(date, _taskDic[skill]);
		}

		public TimeSpan GetForecastedBacklogForIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			var time = TimeSpan.Zero;
			foreach (var task in _taskDic[skill].Values)
			{
				time = time.Add(task.ForecastedBacklogOnDate(date));
			}

			return time;
		}

		public bool IsClosedOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			return _taskDic[skill][date].ClosedDays.Contains(date);
		}

		public TimeSpan? GetManualEntryOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (_manualEntries.ContainsKey(skill))
			{
				if(_manualEntries[skill].ContainsKey(date))
					return _manualEntries[skill][date];
			}

			return null;
		}

		public TimeSpan? GetManualEntryOnDate(DateOnly date, ISkill skill)
		{
			if (_manualEntries.ContainsKey(skill))
			{
				if (_manualEntries[skill].ContainsKey(date))
					return _manualEntries[skill][date];
			}

			return null;
		}

		public void ClearManualEntryOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			clearManualEntryOnBacklogTasks(skill, date);
		}

		public void SetManualEntryOnIndex(int colIndex, ISkill skill, TimeSpan time)
		{
			if (IsClosedOnIndex(colIndex, skill))
				return;

			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			var distributedTime = setManualEntryOnBacklogTasks(skill, time, date);

			if(!_manualEntries.ContainsKey(skill))
				_manualEntries.Add(skill, new Dictionary<DateOnly, TimeSpan>());

			if(!_manualEntries[skill].ContainsKey(date))
				_manualEntries[skill].Add(date, TimeSpan.Zero);

			_manualEntries[skill][date] = distributedTime;
		}

		public void SetManualEntryOnDate(DateOnly date, ISkill skill, TimeSpan time)
		{
			if (_taskDic[skill][date].ClosedDays.Contains(date))
				return;

			var distributedTime = setManualEntryOnBacklogTasks(skill, time, date);

			if (!_manualEntries.ContainsKey(skill))
				_manualEntries.Add(skill, new Dictionary<DateOnly, TimeSpan>());

			if (!_manualEntries[skill].ContainsKey(date))
				_manualEntries[skill].Add(date, TimeSpan.Zero);

			_manualEntries[skill][date] = distributedTime;
		}

		private void clearManualEntryOnBacklogTasks(ISkill skill, DateOnly date)
		{
			foreach (var task in _taskDic[skill].Values)
			{
				if (task.SpanningDateOnlyPeriod().Contains(date))
					task.ClearManualEntry(date);
			}
		}

		private TimeSpan setManualEntryOnBacklogTasks(ISkill skill, TimeSpan time, DateOnly date)
		{
			var incomingTime = time;
			var affectedTasks = new SortedList<DateOnly, BacklogTask>();
			foreach (var task in _taskDic[skill].Values)
			{
				if (task.SpanningDateOnlyPeriod().Contains(date))
					affectedTasks.Add(task.StartDate, task);
			}

			foreach (var affectedTask in affectedTasks.Values)
			{
				var maxTime = affectedTask.ForecastedBacklogOnDate(date.AddDays(-1));
				if (affectedTask.StartDate == date)
					maxTime = affectedTask.ForecastedBacklogOnDate(date);
				if (time <= maxTime)
				{
					affectedTask.SetManualEntry(date, time);
					time = TimeSpan.Zero;
					break;
				}

				affectedTask.SetManualEntry(date, maxTime);
				time = time.Subtract(maxTime);
			}

			return incomingTime.Subtract(time);
		}

		public void TransferSkillDays(ISkill skill)
		{
			var targetSkill = _stateHolder.SchedulingResultState.Skills.First(s => s.SkillType.ForecastSource == ForecastSource.Backoffice);
			var dirtySkillDays = new List<ISkillDay>();
			foreach (var keyValuePair in _taskDic[skill])
			{
				var skilldays = _stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new List<DateOnly> {keyValuePair.Key});
				ISkillDay targetSkillDay = null;
				foreach (var skillday in skilldays)
				{
					if(skillday.Skill == targetSkill)
					targetSkillDay = skillday;
				}

				targetSkillDay.WorkloadDayCollection.First().Tasks = _backlogTaskForecastedTimePerDateCalculator.CalculateForDate(keyValuePair.Key, _taskDic[skill]).TotalHours; 
				dirtySkillDays.Add(targetSkillDay);
			}

			saveSkillDays(dirtySkillDays);
		}

		private void saveSkillDays(IEnumerable<ISkillDay> dirtyList)
		{
			var dirtySkillDays = new List<ISkillDay>();
			dirtySkillDays.AddRange(dirtyList);
			foreach (var skillDay in dirtySkillDays)
			{
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					try
					{
						var skillDayRepository = new SkillDayRepository(uow);
						skillDayRepository.Add(skillDay);
						uow.PersistAll();
					}
					catch (OptimisticLockException)
					{

					}
					catch (ConstraintViolationException)
					{

					}
				}
			}
		}
	}
}