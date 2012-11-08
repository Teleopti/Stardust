using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    public class GroupShiftCategoryBackToLegalStateServiceTest
    {
        private GroupShiftCategoryBackToLegalStateService _target;
        private MockRepository _mockRepository;
        private IRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private IGroupSchedulingService _scheduleService;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _schedulePart;
        private ISchedulingOptions _schedulingOptions;
        private IGroupPersonsBuilder _groupPersonsBuilder;
        private IGroupOptimizerFindMatrixesForGroup _groupOptimerFindMatrixesForGroup;
    	private ITeamSteadyStateHolder _teamSteadyStateHolder;
		private ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
    	private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
    	private IScheduleDictionary _scheduleDictionary;
    	private ISchedulePartModifyAndRollbackService _rollbackService;
    	private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _shiftCategoryBackToLegalService =
                _mockRepository.StrictMock<IRemoveShiftCategoryBackToLegalService>();
            _scheduleService = _mockRepository.StrictMock<IGroupSchedulingService>();
            _groupPersonsBuilder = _mockRepository.StrictMock<IGroupPersonsBuilder>();
			_groupPersonBuilderForOptimization = _mockRepository.StrictMock<IGroupPersonBuilderForOptimization>();
			_target = new GroupShiftCategoryBackToLegalStateService(_shiftCategoryBackToLegalService, _scheduleService, _groupPersonsBuilder);
            _scheduleDayPro = _mockRepository.StrictMock<IScheduleDayPro>();
            _schedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _schedulingOptions = new SchedulingOptions();
            _groupOptimerFindMatrixesForGroup = _mockRepository.DynamicMock<IGroupOptimizerFindMatrixesForGroup>();
        	_teamSteadyStateHolder = _mockRepository.StrictMock<ITeamSteadyStateHolder>();
			_teamSteadyStateMainShiftScheduler = _mockRepository.StrictMock<ITeamSteadyStateMainShiftScheduler>();
        	_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
        	_rollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
			_dateOnly = new DateOnly(2012, 1, 1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyExecute()
        {
            var virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
            var scheduleMatrix = _mockRepository.DynamicMock<IScheduleMatrixPro>();
            var scheduleMatrixList = new List<IScheduleMatrixPro>();
            scheduleMatrixList.Add((scheduleMatrix));
            var person = new Person() ;
            var dateOnly = new DateOnly();
            //var groupPerson = _mockRepository.DynamicMock<IGroupPerson>();
            var dateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, TimeZoneInfo.Utc);
        	var groupPerson = _mockRepository.StrictMock<IGroupPerson>();
        	var groupPersons = new List<IGroupPerson> {groupPerson};

            using (_mockRepository.Record())
            {
                Expect.Call(_shiftCategoryBackToLegalService.Execute(null, _schedulingOptions))
                    .Return(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro, _scheduleDayPro })
                    .IgnoreArguments()
                    .Repeat.Times(2);
                Expect.Call(virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(Limitations());
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(dateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(_groupOptimerFindMatrixesForGroup.Find(person, dateOnly)).IgnoreArguments().Return(scheduleMatrixList).Repeat.AtLeastOnce();
                Expect.Call(scheduleMatrix.Person).Return(person);
                Expect.Call(scheduleMatrix.SchedulePeriod).Return(virtualSchedulePeriod);
                Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly,dateOnly.AddDays(1))).Repeat.AtLeastOnce() ;
                //Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(dateOnly,new List<IPerson>{person}, true, _schedulingOptions)).IgnoreArguments().Repeat.AtLeastOnce();
            	Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, new List<IPerson> {person}, true,_schedulingOptions)).Return(groupPersons).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_scheduleService.ScheduleOneDay(dateOnly, _schedulingOptions,groupPerson,scheduleMatrixList )).IgnoreArguments().Return(true).Repeat.Times(6);

            	Expect.Call(_scheduleDayPro.Day).Return(_dateOnly).Repeat.AtLeastOnce();
            	Expect.Call(_teamSteadyStateHolder.IsSteadyState(groupPerson)).Return(false).Repeat.AtLeastOnce();
            }
            using (_mockRepository.Playback())
            {
				Assert.AreEqual(1, _target.Execute(virtualSchedulePeriod, _schedulingOptions, scheduleMatrixList, _groupOptimerFindMatrixesForGroup, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _scheduleDictionary, _rollbackService, _groupPersonBuilderForOptimization).Count);
            }	
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUseTeamSteadyState()
		{
			//_teamSteadyStates = new Dictionary<Guid, bool> { { _guid, true } };

			var virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
			var scheduleMatrix = _mockRepository.DynamicMock<IScheduleMatrixPro>();
			var scheduleMatrixList = new List<IScheduleMatrixPro>();
			scheduleMatrixList.Add((scheduleMatrix));
			var person = new Person();
			var dateOnly = new DateOnly();
			var dateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, TimeZoneInfo.Utc);
			var groupPerson = _mockRepository.StrictMock<IGroupPerson>();
			var groupPersons = new List<IGroupPerson> { groupPerson };

			using (_mockRepository.Record())
			{
				Expect.Call(_shiftCategoryBackToLegalService.Execute(null, _schedulingOptions))
					.Return(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro, _scheduleDayPro })
					.IgnoreArguments()
					.Repeat.Times(2);
				Expect.Call(virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(Limitations());
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.AtLeastOnce();
				Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(dateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePart.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_groupOptimerFindMatrixesForGroup.Find(person, dateOnly)).IgnoreArguments().Return(scheduleMatrixList).Repeat.AtLeastOnce();
				Expect.Call(scheduleMatrix.Person).Return(person);
				Expect.Call(scheduleMatrix.SchedulePeriod).Return(virtualSchedulePeriod);
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1))).Repeat.AtLeastOnce();
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, new List<IPerson> { person }, true, _schedulingOptions)).Return(groupPersons).IgnoreArguments().Repeat.AtLeastOnce();
				

				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(groupPerson)).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_dateOnly, groupPerson, _scheduleService, null, null,
																			_groupPersonBuilderForOptimization, scheduleMatrixList,
																			_scheduleDictionary)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mockRepository.Playback())
			{
				Assert.AreEqual(1, _target.Execute(virtualSchedulePeriod, _schedulingOptions, scheduleMatrixList, _groupOptimerFindMatrixesForGroup, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _scheduleDictionary, _rollbackService, _groupPersonBuilderForOptimization).Count);
			}	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUseRegularIfTeamSteadyStateFails()
		{
			//_teamSteadyStates = new Dictionary<Guid, bool> { { _guid, true } };

			var virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
			var scheduleMatrix = _mockRepository.DynamicMock<IScheduleMatrixPro>();
			var scheduleMatrixList = new List<IScheduleMatrixPro>();
			scheduleMatrixList.Add((scheduleMatrix));
			var person = new Person();
			var dateOnly = new DateOnly();
            var dateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, TimeZoneInfo.Utc);
			var groupPerson = _mockRepository.StrictMock<IGroupPerson>();
			var groupPersons = new List<IGroupPerson> { groupPerson };

			using (_mockRepository.Record())
			{
				Expect.Call(_shiftCategoryBackToLegalService.Execute(null, _schedulingOptions))
					.Return(new List<IScheduleDayPro> { _scheduleDayPro, _scheduleDayPro, _scheduleDayPro })
					.IgnoreArguments()
					.Repeat.Times(2);
				Expect.Call(virtualSchedulePeriod.ShiftCategoryLimitationCollection()).Return(Limitations());
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.AtLeastOnce();
				Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(dateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePart.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(_groupOptimerFindMatrixesForGroup.Find(person, dateOnly)).IgnoreArguments().Return(scheduleMatrixList).Repeat.AtLeastOnce();
				Expect.Call(scheduleMatrix.Person).Return(person);
				Expect.Call(scheduleMatrix.SchedulePeriod).Return(virtualSchedulePeriod);
				Expect.Call(virtualSchedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1))).Repeat.AtLeastOnce();
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(dateOnly, new List<IPerson> { person }, true, _schedulingOptions)).Return(groupPersons).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.ScheduleOneDay(dateOnly, _schedulingOptions, groupPerson, scheduleMatrixList)).IgnoreArguments().Return(true).Repeat.Times(6);

				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(groupPerson)).Return(true);
				Expect.Call(_teamSteadyStateHolder.IsSteadyState(groupPerson)).Return(false).Repeat.AtLeastOnce();
				Expect.Call(()=>_teamSteadyStateHolder.SetSteadyState(groupPerson, false));
				Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_dateOnly, groupPerson, _scheduleService, null, null,
																			_groupPersonBuilderForOptimization, scheduleMatrixList,
																			_scheduleDictionary)).IgnoreArguments().Return(false);
			}
			using (_mockRepository.Playback())
			{
				Assert.AreEqual(1, _target.Execute(virtualSchedulePeriod, _schedulingOptions, scheduleMatrixList, _groupOptimerFindMatrixesForGroup, _teamSteadyStateHolder, _teamSteadyStateMainShiftScheduler, _scheduleDictionary, _rollbackService, _groupPersonBuilderForOptimization).Count);
			}
		}

        private static ReadOnlyCollection<IShiftCategoryLimitation> Limitations()
        {
            IShiftCategory shiftCategoryA = ShiftCategoryFactory.CreateShiftCategory("CategoryA");
            IShiftCategoryLimitation limitationA = new ShiftCategoryLimitation(shiftCategoryA);
            limitationA.Weekly = false;
            IShiftCategory shiftCategoryB = ShiftCategoryFactory.CreateShiftCategory("CategoryB");
            IShiftCategoryLimitation limitationB = new ShiftCategoryLimitation(shiftCategoryB);
            limitationB.Weekly = false;

            Collection<IShiftCategoryLimitation> limits = new Collection<IShiftCategoryLimitation> { limitationA, limitationB };
            return new ReadOnlyCollection<IShiftCategoryLimitation>(limits);
        }
    }
}
