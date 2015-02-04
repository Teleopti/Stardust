using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
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
		private readonly DateOnlyPeriod _period;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IDictionary<ISkill, IDictionary<DateOnly, BacklogTask>> _taskDic = new Dictionary<ISkill, IDictionary<DateOnly, BacklogTask>>();
		private readonly BacklogTaskForecastedTimePerDateCalculator _backlogTaskForecastedTimePerDateCalculator = new BacklogTaskForecastedTimePerDateCalculator();
		private readonly IDictionary<ISkill, IDictionary<DateOnly, TimeSpan>> _manualEntries = new Dictionary<ISkill, IDictionary<DateOnly, TimeSpan>>();
		private readonly IDictionary<ISkill, ISkill> _skillPairsBackofficeEmail = new Dictionary<ISkill, ISkill>();
		private readonly IDictionary<ISkill, ISkill> _skillPairsEmailBackOffice = new Dictionary<ISkill, ISkill>();
		private bool _loaded;

		public BacklogModel(IComponentContext componentContext, DateOnlyPeriod period)
		{
			_period = period;
			var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
			ILifetimeScope container = lifetimeScope.BeginLifetimeScope();
			_resourceOptimizationHelper = container.Resolve<IResourceOptimizationHelper>();
			_stateHolder = new SchedulerStateHolder(container.Resolve<ISchedulingResultStateHolder>());
		}

		public IDictionary<ISkill, ISkill> SkillPairsBackofficeEmail
		{
			get { return _skillPairsBackofficeEmail; }
		}

		public bool Loaded
		{
			get { return _loaded; }
		}

		public void Load(BackgroundWorker backgroundWorker, DateOnly productPlanStart)
		{
			var dateOnlyPeriodAsDateTimePeriod = new DateOnlyPeriodAsDateTimePeriod(_period, TimeZoneGuard.Instance.TimeZone);
			_stateHolder.RequestedPeriod = dateOnlyPeriodAsDateTimePeriod;
			
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var scenarioRepository = new ScenarioRepository(uow);
				loadCommonStateHolder(uow,_stateHolder);				
				loadSkills(uow, _stateHolder);
				_stateHolder.SetRequestedScenario(scenarioRepository.LoadDefaultScenario());
				backgroundWorker.ReportProgress(0,"Loading forecasts...");
				loadSkillDays(uow, _stateHolder, dateOnlyPeriodAsDateTimePeriod.Period());
				backgroundWorker.ReportProgress(0, "Loading schedules...");
				loadSchedules(uow,_stateHolder);
				
			}
			backgroundWorker.ReportProgress(0, "Calculating resources...");
			foreach (var dateOnly in _period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(dateOnly,false,true);
			}
			
			backgroundWorker.ReportProgress(0, "Initializing...");

			var backOfficeSkills =
				_stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Backoffice).ToList();
			var emailSkills =
				_stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Email).ToList();
			
			var cnt = 0;
			foreach (var backOfficeSkill in backOfficeSkills)
			{
				cnt++;
				if(cnt>emailSkills.Count())
					break;

				SkillPairsBackofficeEmail.Add(backOfficeSkill, emailSkills[cnt - 1]);
				_skillPairsEmailBackOffice.Add(emailSkills[cnt - 1], backOfficeSkill);
			}

			createBacklogTasks(_period);
			setClosedDates(_period);
			setScheduledTime(_period);
			transferScheduledBacklog(emailSkills, productPlanStart);
			_loaded = true;
		}

		public void TransferBacklogs(DateOnly productPlanStart)
		{
			var emailSkills =
				_stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Email).ToList();
			transferScheduledBacklog(emailSkills, productPlanStart);
		}

		private void transferScheduledBacklog(List<ISkill> emailSkills, DateOnly productPlanStart)
		{
			foreach (var dateOnly in _period.DayCollection())
			{
				foreach (var emailSkill in emailSkills)
				{
					if (!_taskDic[emailSkill].ContainsKey(dateOnly))
						continue;

					var taskOnDate = _taskDic[emailSkill][dateOnly];
					DateOnly only = dateOnly;
					var nextStart = _taskDic[emailSkill].Keys.FirstOrDefault(d => d > only);
					if (nextStart == new DateOnly())
						continue;
					var nextTask = _taskDic[emailSkill][nextStart];
					nextTask.BacklogScheduledTask.TransferedBacklog = taskOnDate.BacklogScheduledTask.ScheduledBacklogOnTask();
					nextTask.BacklogProductPlanTask.TransferedBacklog = taskOnDate.BacklogProductPlanTask.PlannedBacklogOnTask();
					if (taskOnDate.StartDate < productPlanStart && nextTask.StartDate >= productPlanStart)
						nextTask.BacklogProductPlanTask.TransferedBacklog =
							nextTask.BacklogProductPlanTask.TransferedBacklog.Add(taskOnDate.BacklogScheduledTask.ScheduledBacklogOnTask());
				}
			}
		}

		private void setScheduledTime(DateOnlyPeriod period)
		{
			foreach (var dateOnly in period.DayCollection())
			{
				foreach (var skillDay in _stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new List<DateOnly> {dateOnly}))
				{
					if (!_skillPairsBackofficeEmail.ContainsKey(skillDay.Skill))
						continue;

					var time =
						TimeSpan.FromHours(SkillStaffPeriodHelper.ScheduledHours(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(0));
					setScheduledTimeOnBacklogTasks(SkillPairsBackofficeEmail[skillDay.Skill], time, dateOnly);
				}
			}
		}

		public TimeSpan? GetScheduledOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			foreach (var skillDay in _stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new List<DateOnly> { date }))
			{
				if (!_skillPairsBackofficeEmail.ContainsKey(skillDay.Skill))
					continue;

				if (skillDay.Skill != _skillPairsEmailBackOffice[skill])
					continue;

				return
					TimeSpan.FromHours(SkillStaffPeriodHelper.ScheduledHours(skillDay.SkillStaffPeriodCollection).GetValueOrDefault(0));
			}

			return TimeSpan.Zero;
		}

		public DateOnlyPeriod Period()
		{
			return _stateHolder.RequestedPeriod.DateOnlyPeriod;
		}

		public int GetIncomingCount(ISkill skill)
		{
			return _taskDic[skill].Count;
		}

		public IEnumerable<ISkill> GetTabSkillList()
		{
			return _stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Email).ToList();
		}

		private void createBacklogTasks(DateOnlyPeriod period)
		{			
			foreach (var skill in _stateHolder.SchedulingResultState.Skills)
			{
				if (skill.SkillType.ForecastSource != ForecastSource.Email)
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

					if (skillDay.ForecastedIncomingDemand == TimeSpan.Zero)
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

		public DateOnly GetDateOnIndex(int colIndex)
		{
			return _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
		}

		public string GetDateStringForIndex(int colIndex)
		{
			var cultureInfo = TeleoptiPrincipal.Current.Regional.Culture;
			return _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex-1].ToShortDateString(cultureInfo);
		}

		public TimeSpan? GetIncomingForIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex-1];
			if (!_taskDic[skill].ContainsKey(date))
				return TimeSpan.Zero;

			return _taskDic[skill][date].BacklogProductPlanTask.TotalIncoming();
		}

		public TimeSpan? GetProductPlanTimeOnIncoming(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return TimeSpan.Zero;

			var task = _taskDic[skill][date].BacklogProductPlanTask;
			return task.PlannedTimeOnTask();
		}

		public TimeSpan? GetProductPlanBacklogOnIncoming(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return TimeSpan.Zero;

			var task = _taskDic[skill][date].BacklogProductPlanTask;
			return task.PlannedBacklogOnTask();
		}

		public TimeSpan GetScheduledOverstaffOnIncomming(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return TimeSpan.Zero;

			var task = _taskDic[skill][date].BacklogScheduledTask;
			return task.OverStaff;
		}

		public TimeSpan? GetForecastedForIndex(int colIndex, ISkill skill)
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
				time = time.Add(task.BacklogProductPlanTask.ForecastedBacklogOnDate(date));
			}

			return time;
		}

		public bool IsClosedOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return true;

			return _taskDic[skill][date].ClosedDays.Contains(date);
		}

		public TimeSpan GetScheduledBookedOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			var time = TimeSpan.Zero;
			foreach (var task in _taskDic[skill].Values)
			{
				time = time.Add(task.BacklogScheduledTask.ScheduledTimeOnDate(date));
			}

			return time;
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

		public void ClearManualEntryOnIndex(int colIndex, ISkill skill, DateOnly productPlanStart)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			clearManualEntryOnBacklogTasks(skill, date);
			if (!_manualEntries.ContainsKey(skill))
				return;

			if (!_manualEntries[skill].ContainsKey(date))
				return;

			_manualEntries[skill].Remove(date);
			var emailSkills =
				_stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Email).ToList();
			transferScheduledBacklog(emailSkills, productPlanStart);
		}

		public void SetManualEntryOnIndex(int colIndex, ISkill skill, TimeSpan time, DateOnly productPlanStart)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			SetManualEntryOnDate(date, skill, time, productPlanStart);
		}

		public void SetManualEntryOnDate(DateOnly date, ISkill skill, TimeSpan time, DateOnly productPlanStart)
		{
			if (_taskDic[skill][date].ClosedDays.Contains(date))
				return;

			var distributedTime = setManualEntryOnBacklogTasks(skill, time, date);

			if (!_manualEntries.ContainsKey(skill))
				_manualEntries.Add(skill, new Dictionary<DateOnly, TimeSpan>());

			if (!_manualEntries[skill].ContainsKey(date))
				_manualEntries[skill].Add(date, TimeSpan.Zero);

			_manualEntries[skill][date] = distributedTime;

			var emailSkills =
				_stateHolder.SchedulingResultState.Skills.Where(s => s.SkillType.ForecastSource == ForecastSource.Email).ToList();
			transferScheduledBacklog(emailSkills, productPlanStart);
		}

		private void clearManualEntryOnBacklogTasks(ISkill skill, DateOnly date)
		{
			foreach (var task in _taskDic[skill].Values)
			{
				if (task.SpanningDateOnlyPeriod().Contains(date))
					task.BacklogProductPlanTask.ClearManualEntry(date);
			}
		}

		private TimeSpan setManualEntryOnBacklogTasks(ISkill skill, TimeSpan time, DateOnly date)
		{
			var affectedTasks = new SortedList<DateOnly, BacklogTask>();
			foreach (var task in _taskDic[skill].Values)
			{
				if (task.SpanningDateOnlyPeriod().Contains(date))
					affectedTasks.Add(task.StartDate, task);
			}

			TimeSpan bookedTime = TimeSpan.Zero;
			TimeSpan timeToBook = time;
			foreach (var affectedTask in affectedTasks.Values)
			{
				var maxTime = affectedTask.BacklogProductPlanTask.ForecastedBacklogOnDate(date.AddDays(-1));
				if (affectedTask.StartDate == date)
					maxTime = affectedTask.BacklogProductPlanTask.TotalIncoming();
				if (timeToBook <= maxTime)
				{
					affectedTask.BacklogProductPlanTask.SetManualEntry(date, timeToBook);
					bookedTime = bookedTime.Add(timeToBook);
				}
				else
				{
					affectedTask.BacklogProductPlanTask.SetManualEntry(date, maxTime);
					bookedTime = bookedTime.Add(maxTime);
				}

				timeToBook = time.Subtract(bookedTime);
				if (timeToBook < TimeSpan.Zero)
					timeToBook = TimeSpan.Zero;
			}

			return bookedTime;
		}

		private void setScheduledTimeOnBacklogTasks(ISkill skill, TimeSpan time, DateOnly date)
		{
			var affectedTasks = new SortedList<DateOnly, BacklogTask>();
			foreach (var task in _taskDic[skill].Values)
			{
				if (task.SpanningDateOnlyPeriod().Contains(date))
					affectedTasks.Add(task.StartDate, task);
			}

			TimeSpan bookedTime = TimeSpan.Zero;
			TimeSpan timeToBook = time;
			foreach (var affectedTask in affectedTasks.Values)
			{
				var maxTime = affectedTask.BacklogScheduledTask.ScheduledBackLogOnDate(date);
				if (timeToBook <= maxTime)
				{
					affectedTask.BacklogScheduledTask.SetScheduledTime(date, timeToBook);
					bookedTime = bookedTime.Add(timeToBook);
				}
				else
				{
					affectedTask.BacklogScheduledTask.SetScheduledTime(date, maxTime);
					bookedTime = bookedTime.Add(maxTime);
				}

				timeToBook = time.Subtract(bookedTime);
			}

			if (timeToBook > TimeSpan.Zero)
				affectedTasks.Values.Last().BacklogScheduledTask.OverStaff = timeToBook;
		}

		public void TransferSkillDays(ISkill skill)
		{
			ISkill targetSkill = null;
			foreach (var skillPair in _skillPairsBackofficeEmail)
			{
				if (skillPair.Value == skill)
					targetSkill = skillPair.Key;

			}

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

		private void loadSchedules(IUnitOfWork uow, ISchedulerStateHolder stateHolder)
		{
			var period = new ScheduleDateTimePeriod(stateHolder.RequestedPeriod.Period(), stateHolder.SchedulingResultState.PersonsInOrganization);
			using (PerformanceOutput.ForOperation("Loading schedules " + period.LoadedPeriod()))
			{
				stateHolder.SchedulingResultState.PersonsInOrganization = new PersonRepository(uow).FindPeopleInOrganization(Period(), false);
				IPersonProvider personsInOrganizationProvider = new PersonsInOrganizationProvider(stateHolder.SchedulingResultState.PersonsInOrganization);
				// If the people in organization is filtered out to 70% or less of all people then flag 
				// so that a criteria for that is used later when loading schedules.
				//var loaderSpecification = new LoadScheduleByPersonSpecification();
				//personsInOrganizationProvider.DoLoadByPerson = loaderSpecification.IsSatisfiedBy(decider);
				IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false)
				{
					LoadDaysAfterLeft = true
				};
				stateHolder.LoadSchedules(new ScheduleRepository(uow), personsInOrganizationProvider, scheduleDictionaryLoadOptions, period);
			}
		}

		public TimeSpan? GetScheduledOnIncomingIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return TimeSpan.Zero;

			return _taskDic[skill][date].BacklogScheduledTask.ScheduledTimeOnTask();
		}

		public TimeSpan? GetScheduledBacklogOnIncomingIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return TimeSpan.Zero;

			return _taskDic[skill][date].BacklogScheduledTask.ScheduledBacklogOnTask();
		}

		public Percent GetScheduledServiceLevelOnIncomingIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			if (!_taskDic[skill].ContainsKey(date))
				return new Percent();

			return _taskDic[skill][date].BacklogScheduledTask.ScheduledServiceLevelOnTask();
		}

		public TimeSpan? GetScheduledTimeOnTaskForDate(int colIndex, int rowIndex, int fixedRows, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			var incomingList = _taskDic[skill].Keys.ToList();
			var incomingDate = incomingList[rowIndex - fixedRows - 1];
			if (rowIndex - fixedRows > incomingList.Count)
				return null;

			var task = _taskDic[skill][incomingDate];
			if (!task.SpanningDateOnlyPeriod().Contains(date))
				return null;

			return task.BacklogScheduledTask.ScheduledTimeOnDate(date);
		}

		public TimeSpan? GetPlannedTimeOnTaskForDate(int colIndex, int rowIndex, int fixedRows, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			var incomingList = _taskDic[skill].Keys.ToList();
			var incomingDate = incomingList[rowIndex - fixedRows - 1];
			if (rowIndex - fixedRows > incomingList.Count)
				return null;

			var task = _taskDic[skill][incomingDate];
			if (!task.SpanningDateOnlyPeriod().Contains(date))
				return null;

			return task.BacklogProductPlanTask.ForecastedTimeOnDate(date);
		}

		public TimeSpan? GetScheduledBacklogOnIndex(int colIndex, ISkill skill)
		{
			var date = _stateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection()[colIndex - 1];
			var time = TimeSpan.Zero;
			foreach (var task in _taskDic[skill].Values)
			{
				time = time.Add(task.BacklogScheduledTask.ScheduledBackLogOnDate(date));
			}

			return time;
		}
	}
}