using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface ISchedulerStateLoader
	{
		void LoadOrganization();
		void LoadSchedules(IScheduleDateTimePeriod scheduleDateTimePeriod);
		void LoadSchedulingResultAsync(IScheduleDateTimePeriod scheduleDateTimePeriod, BackgroundWorker backgroundWorker, IEnumerable<ISkill> skills, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade);
		void EnsureSkillsLoaded(DateOnlyPeriod period);
	}

	public class SchedulerStateLoader : ISchedulerStateLoader
	{
		private readonly SchedulingScreenState _schedulerState;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILazyLoadingManager _lazyManager;
		private readonly IScheduleStorageFactory _scheduleStorageFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IPersonSkillProvider _personSkillProvider = new PersonSkillProvider();
		private BackgroundWorker _backgroundWorker;
		
		public SchedulerStateLoader(SchedulingScreenState stateHolder, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory uowFactory, ILazyLoadingManager lazyManager, IScheduleStorageFactory scheduleStorageFactory)
		{
			_unitOfWorkFactory = uowFactory;
			_lazyManager = lazyManager;
			_scheduleStorageFactory = scheduleStorageFactory;
			_repositoryFactory = repositoryFactory;
			_schedulerState = stateHolder;
		}

		public void LoadOrganization()
		{
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				initializeCommonState(uow);
				uow.DisableFilter(QueryFilter.Deleted);
				initializePeople(uow);
			}
		}

		public void LoadSchedules(IScheduleDateTimePeriod scheduleDateTimePeriod)
		{
			if (_schedulerState.SchedulerStateHolder.Schedules != null && _schedulerState.SchedulerStateHolder.RequestedPeriod.Period().Contains(scheduleDateTimePeriod.RangeToLoadCalculator.RequestedPeriod))
				return;
			var timeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (_schedulerState.SchedulerStateHolder.RequestedPeriod.Period() != scheduleDateTimePeriod.RangeToLoadCalculator.RequestedPeriod)
				{
					((SchedulerStateHolder)_schedulerState.SchedulerStateHolder).RequestedPeriod =
						new DateOnlyPeriodAsDateTimePeriod(
							scheduleDateTimePeriod.RangeToLoadCalculator.RequestedPeriod.ToDateOnlyPeriod(timeZone), timeZone);
				}
				_schedulerState.SchedulerStateHolder.SchedulingResultState.SkillDays = null;
				reassociateCommonData(uow);
				reassociatePeople(uow);

				initializeSchedules(uow, scheduleDateTimePeriod);
			}
		}

		public void LoadSchedulingResultAsync(IScheduleDateTimePeriod scheduleDateTimePeriod, BackgroundWorker backgroundWorker, IEnumerable<ISkill> skills, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			_backgroundWorker = backgroundWorker;

			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				reassociateCommonData(uow);
				if (_backgroundWorker.CancellationPending)
					return;

				reassociatePeople(uow);
				if (_backgroundWorker.CancellationPending)
					return;

				if (_schedulerState.SchedulerStateHolder.Schedules == null ||
					_schedulerState.SchedulerStateHolder.RequestedPeriod.Period() !=
					scheduleDateTimePeriod.RangeToLoadCalculator.RequestedPeriod)
				{
					var timeZone = TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone();
					((SchedulerStateHolder) _schedulerState.SchedulerStateHolder).RequestedPeriod =
						new DateOnlyPeriodAsDateTimePeriod(
							scheduleDateTimePeriod.RangeToLoadCalculator.RequestedPeriod.ToDateOnlyPeriod(timeZone),
							timeZone);
					initializeSchedules(uow, scheduleDateTimePeriod);
				}

				if (_backgroundWorker.CancellationPending)
					return;
				initializeSkills(uow);
				if (_backgroundWorker.CancellationPending)
					return;
				initializeSkillDays(uow, skills, staffingCalculatorServiceFacade);
				if (_backgroundWorker.CancellationPending)
					return;
				initializeScheduleData();
			}
		}

		public void EnsureSkillsLoaded(DateOnlyPeriod period)
		{
			if (!_schedulerState.SchedulerStateHolder.SchedulingResultState.Skills.IsEmpty())
				return;

			using (PerformanceOutput.ForOperation("Loading skills"))
			{
				using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					var service = _repositoryFactory.CreateSkillRepository(uow);
					var skills = service.FindAllWithSkillDays(period);

					foreach (var skill in skills)
					{
						LazyLoadingManager.Initialize(skill.SkillType);
						var oldSkillsPlusNewSkill =
							new HashSet<ISkill>(_schedulerState.SchedulerStateHolder.SchedulingResultState.Skills) {skill};
						_schedulerState.SchedulerStateHolder.SchedulingResultState.Skills = oldSkillsPlusNewSkill;
					}
				}
			}
		}

		private void initializeCommonState(IUnitOfWork uow)
		{
			using (PerformanceOutput.ForOperation("Loading common data"))
			{
				_schedulerState.SchedulerStateHolder.CommonStateHolder.LoadCommonStateHolder(_repositoryFactory, uow);
			}
		}

		private void initializePeople(IUnitOfWork uow)
		{
			using (PerformanceOutput.ForOperation("Loading people"))
			{
				IBusinessUnitRepository businessUnitRepository = _repositoryFactory.CreateBusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName(); //Load the business units into this uow
				IPersonRepository service = _repositoryFactory.CreatePersonRepository(uow);

				_schedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents =
					service.FindAllAgents(_schedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod, true);

				foreach (IPerson person in _schedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents)
				{
					if (!_schedulerState.SchedulerStateHolder.ChoosenAgents.Contains(person))
					{
						_schedulerState.SchedulerStateHolder.ChoosenAgents.Add(person);
					}
				}
				_schedulerState.SchedulerStateHolder.ResetFilteredPersons();
			}
		}

		private void reassociateCommonData(IUnitOfWork uow)
		{
			using (PerformanceOutput.ForOperation("Reassociating common data"))
			{
				uow.Reassociate(_schedulerState.SchedulerStateHolder.CommonStateHolder.Absences);
				if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
					return;
				uow.Reassociate(_schedulerState.SchedulerStateHolder.CommonStateHolder.Activities);
				if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
					return;
				uow.Reassociate(_schedulerState.SchedulerStateHolder.CommonStateHolder.DayOffs);
				if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
					return;
				uow.Reassociate(_schedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories);
				if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
					return;
				uow.Reassociate(_schedulerState.SchedulerStateHolder.RequestedScenario);
				uow.Reassociate(_schedulerState.SchedulerStateHolder.CommonStateHolder.MultiplicatorDefinitionSets);
			}
		}

		private void reassociatePeople(IUnitOfWork uow)
		{
			using (PerformanceOutput.ForOperation("Reassociating people"))
			{
				uow.Reassociate(_schedulerState.SchedulerStateHolder.ChoosenAgents);
			}
		}

		private void initializeSkills(IUnitOfWork uow)
		{
			using (PerformanceOutput.ForOperation("Loading skills"))
			{
				ISkillRepository service = _repositoryFactory.CreateSkillRepository(uow);
				var skills =
					service.FindAllWithSkillDays(_schedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod)
						.ForEach(s => _lazyManager.Initialize(s.SkillType))
						.ToArray();
				_schedulerState.SchedulerStateHolder.SchedulingResultState.Skills = new HashSet<ISkill>(skills);
			}
		}

		private void initializeSkillDays(IUnitOfWork uow, IEnumerable<ISkill> skills, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			using (PerformanceOutput.ForOperation("Loading skill days (intraday data)"))
			{
				_schedulerState.SchedulerStateHolder.SchedulingResultState.SkillDays = new SkillDayLoadHelper(
					_repositoryFactory.CreateSkillDayRepository(uow),
					_repositoryFactory.CreateMultisiteDayRepository(uow), staffingCalculatorServiceFacade).LoadSchedulerSkillDays(
					_schedulerState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
					skills,
					_schedulerState.SchedulerStateHolder.RequestedScenario);
			}
		}

		private void initializeSchedules(IUnitOfWork uow, IScheduleDateTimePeriod scheduleDateTimePeriod)
		{
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);
			IScheduleStorage scheduleStorage = _scheduleStorageFactory.Create(uow);

			using (PerformanceOutput.ForOperation("Loading schedules"))
			{
				uow.Reassociate(_schedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents);
				using (uow.DisableFilter(QueryFilter.Deleted))
					_repositoryFactory.CreateActivityRepository(uow).LoadAll();
				_schedulerState.SchedulerStateHolder.LoadSchedules(scheduleStorage, _schedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents, scheduleDictionaryLoadOptions, scheduleDateTimePeriod.VisiblePeriod);

				var period = scheduleDateTimePeriod.RangeToLoadCalculator.RequestedPeriod.ToDateOnlyPeriod(TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone());
				foreach (var scheduleRange in _schedulerState.SchedulerStateHolder.Schedules.Values)
				{
					scheduleRange.ScheduledDayCollection(period).ForEach(x => _lazyManager.Initialize(x.PersonAssignment(true).DayOffTemplate));
				}
			}
		}

		private void initializeScheduleData()
		{
			using (PerformanceOutput.ForOperation("Loading schedule data"))
			{
				var service =
					new SchedulingResultService(_schedulerState.SchedulerStateHolder.SchedulingResultState, _schedulerState.SchedulerStateHolder.SchedulingResultState.Skills, _personSkillProvider);
				service.SchedulingResult(_schedulerState.SchedulerStateHolder.RequestedPeriod.Period(),null, true);
			}
		}
	}
}
