using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Rhino.Mocks;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class SchedulerStateLoaderTest
    {
        private SchedulerStateLoader _targetStateLoader;
        private ISchedulerStateHolder _targetStateHolder;
        private IScenario _targetScenario;
        private readonly DateOnlyPeriod _targetPeriod = new DateOnlyPeriod(2008, 10, 20, 2008, 10, 21);
        private IList<IPerson> _permittedPeople;
        private ISkill _selectedSkill;
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
    	private ILazyLoadingManager _lazyManager;
	    private DateTimePeriod _period;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _permittedPeople = new List<IPerson> { PersonFactory.CreatePerson() };

			_period = _targetPeriod.ToDateTimePeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _targetScenario = _mocks.StrictMock<IScenario>();
            _selectedSkill = _mocks.StrictMock<ISkill>();
			_targetStateHolder = new SchedulerStateHolder(_targetScenario, new DateOnlyPeriodAsDateTimePeriod(_targetPeriod, TimeZoneInfoFactory.UtcTimeZoneInfo()), _permittedPeople);
        	_lazyManager = _mocks.DynamicMock<ILazyLoadingManager>();
        }

        [Test]
        public void VerifyInstanceIsCreatedWithoutRepositoryFactory()
        {
            _targetStateLoader = new SchedulerStateLoader(_targetStateHolder);
            Assert.IsNotNull(_targetStateLoader);
        }

        [Test]
        public void VerifyInstanceIsCreatedWithRepositoryFactory()
        {
			_targetStateLoader = new SchedulerStateLoader(_targetStateHolder, _repositoryFactory, _unitOfWorkFactory, _lazyManager);
            Assert.IsNotNull(_targetStateLoader);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void VerifyLoadSchedulingResult()
        {
			_targetStateLoader = new SchedulerStateLoader(_targetStateHolder, _repositoryFactory, _unitOfWorkFactory, _lazyManager);
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var disposable = _mocks.StrictMock<IDisposable>();
            var activityRepository = _mocks.StrictMock<IActivityRepository>();
            
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.AtLeastOnce();
            uow.Dispose();
            LastCall.Repeat.AtLeastOnce();

            CreateForecastInitializeExpectation(uow, _selectedSkill, _targetPeriod);
            LastCall.IgnoreArguments().Repeat.Once();

            IScheduleDictionary scheduleDictionary = CreateScheduleInitializationExpectation(uow);

            PrepareSkill(_selectedSkill);

            uow.Reassociate(_targetStateHolder.AllPermittedPersons);
            LastCall.Repeat.AtLeastOnce();

            uow.Reassociate(_targetStateHolder.CommonStateHolder.Activities);
            LastCall.Repeat.AtLeastOnce();

            uow.Reassociate(_targetStateHolder.CommonStateHolder.Absences);
            LastCall.Repeat.AtLeastOnce();

            uow.Reassociate(_targetStateHolder.CommonStateHolder.ShiftCategories);
            LastCall.Repeat.AtLeastOnce();

            uow.Reassociate(_targetStateHolder.CommonStateHolder.DayOffs);
            LastCall.Repeat.AtLeastOnce();

            uow.Reassociate(_targetScenario);
            LastCall.Repeat.AtLeastOnce();

            disposable.Dispose();
            LastCall.Repeat.AtLeastOnce();
            Expect.Call(uow.DisableFilter(QueryFilter.Deleted)).Return(disposable);

            Expect.Call(_repositoryFactory.CreateActivityRepository(uow)).Return(activityRepository);
            Expect.Call(activityRepository.LoadAll()).Return(new List<IActivity>());

            uow.Reassociate(_targetStateHolder.SchedulingResultState.PersonsInOrganization);

            _mocks.ReplayAll();

            var scheduleDateTimePeriod =
                new ScheduleDateTimePeriod(_period);
            _targetStateLoader.LoadSchedules(scheduleDateTimePeriod);
            _targetStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
            _targetStateLoader.LoadSchedulingResultAsync(scheduleDateTimePeriod, uow, new BackgroundWorker(), new List<ISkill> { _selectedSkill });

            _mocks.VerifyAll();

            Assert.IsTrue(_targetStateHolder.SchedulingResultState.Skills.Contains(_selectedSkill));
            Assert.IsTrue(_targetStateHolder.AllPermittedPersons.Contains(_permittedPeople[0]));
            Assert.AreSame(scheduleDictionary, _targetStateHolder.SchedulingResultState.Schedules);
        }

        private IScheduleDictionary CreateScheduleInitializationExpectation(IUnitOfWork uow)
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var scheduleRepository = _mocks.StrictMock<IScheduleRepository>();
	        var range = _mocks.StrictMock<IScheduleRange>();
	        var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            Expect.Call(_repositoryFactory.CreateScheduleRepository(uow)).Return(scheduleRepository);
            Expect.Call(scheduleRepository.FindSchedulesForPersons(null, null, null, null, null))
                .IgnoreArguments()
                .Return(scheduleDictionary);
	        Expect.Call(scheduleDictionary.Keys).Return(_permittedPeople);
	        Expect.Call(scheduleDictionary[_permittedPeople[0]]).Return(range);
	        Expect.Call(range.TotalPeriod()).Return(_period);
	        Expect.Call(range.ScheduledDayCollection(_targetPeriod)).Return(new[] {scheduleDay, scheduleDay});
	        Expect.Call(scheduleDay.ProjectionService())
	              .Return(new VisualLayerProjectionService(_permittedPeople[0]))
	              .Repeat.AtLeastOnce();

            return scheduleDictionary;
        }

        private void CreateForecastInitializeExpectation(IUnitOfWork uow, ISkill skill, DateOnlyPeriod period)
        {
            var skillRepository = _mocks.StrictMock<ISkillRepository>();
            var skillDayRepository = _mocks.StrictMock<ISkillDayRepository>();
            var multisiteDayRepository = _mocks.StrictMock<IMultisiteDayRepository>();

            Expect.Call(_repositoryFactory.CreateSkillDayRepository(uow)).Return(skillDayRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateMultisiteDayRepository(uow)).Return(multisiteDayRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateSkillRepository(uow)).Return(skillRepository).Repeat.Once();

            Expect.Call(skillRepository.FindAllWithSkillDays(_targetPeriod)).Return(new List<ISkill> { skill }).Repeat.Once();
            Expect.Call(
                skillDayRepository.FindRange(period, new List<ISkill> { skill }, _targetScenario))
                .Return(new List<ISkillDay>()).Repeat.Once();
        }

        private void PrepareSkill(ISkill skill)
        {
            var skillType = _mocks.StrictMock<ISkillType>();
            IActivity activity = ActivityFactory.CreateActivity("testact");
            Expect.Call(skill.TimeZone).Return((TimeZoneInfo.Utc)).Repeat.Any();
            Expect.Call(skill.DefaultResolution).Return(15).Repeat.Any();
            Expect.Call(skill.Activity).Return(activity).Repeat.Any();
            Expect.Call(skill.SkillType).Return(skillType).Repeat.Any();
            Expect.Call(skillType.ForecastSource).Return(ForecastSource.InboundTelephony).Repeat.Any();
        }
    }

}
