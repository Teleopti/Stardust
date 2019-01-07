using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class ScheduleTargetTimeCalculatorTest
	{
		private ScheduleTargetTimeCalculator _target;
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
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
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
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mockRepository.StrictMock<IScheduleRange>();
		}

		[Test]
        public void ShouldThrowExceptionIfPersonNull()
        {
            _person = null;
			Assert.Throws<ArgumentNullException>(() => _target = new ScheduleTargetTimeCalculator(_scheduleDictionary, _person, _dateOnlyPeriod));
        }

        [Test]
        public void ShouldThrowExceptionIfStateHolderNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target = new ScheduleTargetTimeCalculator(null, _person, _dateOnlyPeriod));
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
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
				Expect.Call(_contract.WorkTime).Return(WorkTime.DefaultWorkTime).Repeat.AtLeastOnce();
				Expect.Call(_contract.WorkTimeSource).Return(WorkTimeSource.FromContract).Repeat.AtLeastOnce();
				Expect.Call(_contractSchedule.IsWorkday(dateOnly, dateOnly, DayOfWeek.Monday)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}
			using (_mockRepository.Playback())
			{
				_target = new ScheduleTargetTimeCalculator(_scheduleDictionary, _person, _dateOnlyPeriod);
				Assert.AreEqual(TimeSpan.FromHours(56), _target.CalculateTargetTime());
			}
		}
	}
}
