using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.DayOff
{
    [TestFixture]
    public class ValidNumberOfDayOffInAWeekSpecificationTest
    {
        private IValidNumberOfDayOffInAWeekSpecification _target;
        private IScheduleMatrixPro _matrix;
        private DateOnlyPeriod _period;
        private MockRepository _mock;
        private IScheduleDayPro _workingDayScheduleDayPro;
        private IScheduleDayPro _dayOffScheduleDayPro;
        private IScheduleDay _workingScheduleDay;
        private IScheduleDay _dayOffScheduleDay;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _matrix = _mock.StrictMock<IScheduleMatrixPro>();
            _period = new DateOnlyPeriod(2014,02,17,2014,02,23);
            _workingDayScheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _dayOffScheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _workingScheduleDay = _mock.StrictMock<IScheduleDay>();
            _dayOffScheduleDay = _mock.StrictMock<IScheduleDay>();

            _target = new ValidNumberOfDayOffInAWeekSpecification();
        }

        private void commonMock(DateOnly dateOnly, IScheduleDayPro scheduleDayPro, IScheduleDay scheduleDay, SchedulePartView schedulePartView)
        {
            Expect.Call(_matrix.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(schedulePartView);
        }

        [Test]
        public void ShouldReturnTrueIfCorrectNumberOfDayOffExists()
        {
            using (_mock.Record())
            {
                commonMock(new DateOnly(2014, 02, 17), _workingDayScheduleDayPro, _workingScheduleDay,SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 18), _workingDayScheduleDayPro, _workingScheduleDay,SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 19), _workingDayScheduleDayPro, _workingScheduleDay,SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 20), _workingDayScheduleDayPro, _workingScheduleDay,SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 21), _workingDayScheduleDayPro, _workingScheduleDay,SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 22), _dayOffScheduleDayPro , _dayOffScheduleDay ,SchedulePartView.ContractDayOff );
                commonMock(new DateOnly(2014, 02, 23), _dayOffScheduleDayPro , _dayOffScheduleDay ,SchedulePartView.ContractDayOff );

            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfied(_matrix, _period, 2));
            }
        }

        [Test]
        public void ShouldReturnTrueIfNoDayOffShouldExists()
        {
            using (_mock.Record())
            {
                commonMock(new DateOnly(2014, 02, 17), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 18), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 19), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 20), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 21), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 22), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 23), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);

            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfied(_matrix, _period, 0));
            }
        }

        [Test]
        public void ShouldReturnFalseIfDayOffAreLessThanProvidedDayOff()
        {
            using (_mock.Record())
            {
                commonMock(new DateOnly(2014, 02, 17), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 18), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 19), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 20), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 21), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 22), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 23), _dayOffScheduleDayPro, _dayOffScheduleDay, SchedulePartView.ContractDayOff);

            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfied(_matrix, _period, 2));
            }
        }

        [Test]
        public void ShouldReturnTrueIfDayOffAreGreaterThanProvidedDayOff()
        {
            using (_mock.Record())
            {
                commonMock(new DateOnly(2014, 02, 17), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 18), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 19), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 20), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 21), _workingDayScheduleDayPro, _workingScheduleDay, SchedulePartView.MainShift);
                commonMock(new DateOnly(2014, 02, 22), _dayOffScheduleDayPro, _dayOffScheduleDay, SchedulePartView.ContractDayOff);
                commonMock(new DateOnly(2014, 02, 23), _dayOffScheduleDayPro, _dayOffScheduleDay, SchedulePartView.ContractDayOff);

            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfied(_matrix, _period, 1));
            }
        }

    }

    
}
