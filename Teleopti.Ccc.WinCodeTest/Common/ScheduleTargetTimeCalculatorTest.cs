using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
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
		private IList<ISchedulePeriod> _schedulePeriods;
		private IPersonPeriod _personPeriod;
		private IPersonContract _personContract;
		private IContract _contract;
		private IPartTimePercentage _partTimePercentage;
		private IContractSchedule _contractSchedule;
		private ITeam _team;
		private IPermissionInformation _permissionInformation;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_schedulerStateHolder = _mockRepository.StrictMock<ISchedulerStateHolder>();
			_dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1);
			_person = _mockRepository.StrictMock<IPerson>();
			_schedulePeriod = new SchedulePeriod(new DateOnly(2011, 1, 1), SchedulePeriodType.Week, 1);
			_schedulePeriod.SetParent(_person);
			_schedulePeriods = new List<ISchedulePeriod> { _schedulePeriod };
			_contract = _mockRepository.StrictMock<IContract>();
			_partTimePercentage = _mockRepository.StrictMock<IPartTimePercentage>();
			_contractSchedule = _mockRepository.StrictMock<IContractSchedule>();
			_personContract = new PersonContract(_contract, _partTimePercentage, _contractSchedule);
			_team = _mockRepository.StrictMock<ITeam>();
			_personPeriod = new PersonPeriod(new DateOnly(2011, 1, 1), _personContract, _team);
			_permissionInformation = _mockRepository.StrictMock<IPermissionInformation>();
			_schedulingResultStateHolder = _mockRepository.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mockRepository.StrictMock<IScheduleRange>();

            }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfPersonNull()
        {
            _person = null;
            _target = new ScheduleTargetTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfStateHolderNull()
        {
            _schedulerStateHolder = null;
            _target = new ScheduleTargetTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod);
        }

	    [Test]
		public void ShouldCalculate()
		{

            _target = new ScheduleTargetTimeCalculator(_schedulerStateHolder, _person, _dateOnlyPeriod);

			using (_mockRepository.Record())
			{
				Expect.Call(_person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
				Expect.Call(_person.Period(new DateOnly(2011, 1, 1))).IgnoreArguments().Return(_personPeriod).Repeat.AtLeastOnce();
			    Expect.Call(_person.FirstDayOfWeek).Return(DayOfWeek.Monday).Repeat.Twice();
				Expect.Call(_person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.Culture()).Return(CultureInfo.CurrentCulture).Repeat.AtLeastOnce();
				Expect.Call(_contract.MinTimeSchedulePeriod).Return(TimeSpan.FromHours(20));
				Expect.Call(_person.TerminalDate).Return(null);
				Expect.Call(_person.NextPeriod(_personPeriod)).Return(null);
				Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
				Expect.Call(_contractSchedule.IsWorkday(new DateOnly(2011, 1, 1), new DateOnly(2011, 1, 1))).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
				Expect.Call(_partTimePercentage.Percentage).Return(new Percent(1.0));
				Expect.Call(_contract.WorkTime).Return(new WorkTime(TimeSpan.FromHours(8)));
			    Expect.Call(_person.PreviousPeriod(_personPeriod)).Return(null);
				Expect.Call(_person.SchedulePeriodStartDate(new DateOnly(2011, 1, 1))).Return(new DateOnly(2011, 1, 1)).Repeat.AtLeastOnce();
			}

			using (_mockRepository.Playback())
			{
				Assert.AreEqual(TimeSpan.FromHours(56), _target.CalculateTargetTime());
			}
		}
	}
}
