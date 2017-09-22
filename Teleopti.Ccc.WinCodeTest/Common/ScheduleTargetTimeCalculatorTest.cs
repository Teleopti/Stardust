﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class ScheduleTargetTimeCalculatorTest
	{
		private ScheduleTargetTimeCalculator _target;
		private ISchedulerStateHolder _schedulerStateHolder;
		private MockRepository _mockRepository;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IPerson _person;
		private ISchedulePeriod _schedulePeriod;
		private IPersonPeriod _personPeriod;
		private IPersonContract _personContract;
		private IContract _contract;
		private IPartTimePercentage _partTimePercentage;
		private IContractSchedule _contractSchedule;
		private ITeam _team;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_schedulerStateHolder = _mockRepository.StrictMock<ISchedulerStateHolder>();
			_dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1);
			_person = new Person();
			_schedulePeriod = new SchedulePeriod(new DateOnly(2011, 1, 1), SchedulePeriodType.Week, 1);
			_schedulePeriod.SetParent(_person);
			_contract = _mockRepository.StrictMock<IContract>();
			_partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
			_contractSchedule = _mockRepository.StrictMock<IContractSchedule>();
			_personContract = new PersonContract(_contract, _partTimePercentage, _contractSchedule);
			_team = TeamFactory.CreateSimpleTeam();
			_personPeriod = new PersonPeriod(new DateOnly(2011, 1, 1), _personContract, _team);
			_schedulingResultStateHolder = _mockRepository.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mockRepository.StrictMock<IScheduleRange>();
		}

		[Test]
        public void ShouldThrowExceptionIfPersonNull()
        {
            _person = null;
			Assert.Throws<ArgumentNullException>(() => _target = new ScheduleTargetTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod));
        }

        [Test]
        public void ShouldThrowExceptionIfStateHolderNull()
        {
            _schedulerStateHolder = null;
			Assert.Throws<ArgumentNullException>(() => _target = new ScheduleTargetTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod));
        }

	    [Test]
		public void ShouldCalculate()
		{
		    _schedulePeriod.AverageWorkTimePerDayOverride = new TimeSpan(8, 0, 0);
			_person.AddSchedulePeriod(_schedulePeriod);
			_person.AddPersonPeriod(_personPeriod);
		    _person.FirstDayOfWeek = DayOfWeek.Monday;
	        var dateOnly = new DateOnly(2011, 1, 1);
			using (_mockRepository.Record())
			{
				Expect.Call(_contract.MinTimeSchedulePeriod).Return(TimeSpan.FromHours(20));
				Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
				Expect.Call(_contract.WorkTime).Return(WorkTime.DefaultWorkTime).Repeat.AtLeastOnce();
				Expect.Call(_contract.WorkTimeSource).Return(WorkTimeSource.FromContract).Repeat.AtLeastOnce();
				Expect.Call(_contractSchedule.IsWorkday(dateOnly, dateOnly)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mockRepository.Playback())
			{
				_target = new ScheduleTargetTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod);
				Assert.AreEqual(TimeSpan.FromHours(56), _target.CalculateTargetTime());
			}
		}
	}
}
