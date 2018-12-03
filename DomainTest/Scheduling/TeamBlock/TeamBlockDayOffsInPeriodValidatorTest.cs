using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockDayOffsInPeriodValidatorTest
	{
		private TeamBlockDayOffsInPeriodValidator _target;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private MockRepository _mock;
		private ITeamInfo _teamInfo;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _matrixlist;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private IContract _contract;
		private IPerson _person;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleDay _scheduleDay3;
		private IList<IScheduleDay> _scheduleDays;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_target = new TeamBlockDayOffsInPeriodValidator();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixlist = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
			_virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
			_contract = new Contract("contract") {EmploymentType = EmploymentType.FixedStaffNormalWorkTime};
			_person = _mock.StrictMock<IPerson>();
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_dateOnlyPeriod = new DateOnlyPeriod(2015, 1, 1, 2015, 1, 3);
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay3 = _mock.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2, _scheduleDay3};
		}

		[Test]
		public void ShouldReturnTrueIfCorrectNumberOfDayOffs()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixlist);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.Contract).Return(_contract);
				Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
				Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(_scheduleDays);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfCorrectNumberOfDayOffsWithPositiveFlexibility()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixlist);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.Contract).Return(_contract);
				Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
				Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(_scheduleDays);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mock.Playback())
			{
				_contract.PositiveDayOffTolerance = 1;
				var result = _target.Validate(_teamInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfCorrectNumberOfDayOffsWithNegativeFlexibility()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixlist);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.Contract).Return(_contract);
				Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
				Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(_scheduleDays);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mock.Playback())
			{
				_contract.NegativeDayOffTolerance = 1;
				var result = _target.Validate(_teamInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfNotCorrectNumberOfDayOffs()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixlist);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.Contract).Return(_contract);
				Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
				Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleRange.ScheduledDayCollection(_dateOnlyPeriod)).Return(_scheduleDays);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mock.Playback())
			{
				var result = _target.Validate(_teamInfo, _schedulingResultStateHolder);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfHourlyStaff()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroup()).Return(_matrixlist);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.Contract).Return(_contract);
			}

			using (_mock.Playback())
			{
				_contract.EmploymentType = EmploymentType.HourlyStaff;
				var result = _target.Validate(_teamInfo, _schedulingResultStateHolder);
				Assert.IsTrue(result);
			}
		}
	}
}
