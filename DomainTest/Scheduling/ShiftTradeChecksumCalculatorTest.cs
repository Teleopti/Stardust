using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class ShiftTradeChecksumCalculatorTest
    {
        private ShiftTradeChecksumCalculator _target;
        private MockRepository _mocks;
        private IScheduleDay _schedulePart;


        [Test]
        public void VerifyChecksumIsSameForUnchangedPersonAssignment()
        {
            _mocks = new MockRepository();
            _schedulePart = _mocks.StrictMock<IScheduleDay>();
            _target = new ShiftTradeChecksumCalculator(_schedulePart);

            var period = new DateTimePeriod(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2009, 1, 1, 9, 0, 0, DateTimeKind.Utc));
            IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(),
                                                                          ScenarioFactory.CreateScenarioAggregate(), period);

            using (_mocks.Record())
            {
                Expect.Call(_schedulePart.PersonAssignment()).Return(personAssignment).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                long checksum1 = _target.CalculateChecksum();
                long checksum2 = _target.CalculateChecksum();

                Assert.AreEqual(checksum1, checksum2);
            }
        }

        [Test]
        public void ShouldNotCalculateChecksumWhenNoDayOffAndNoMainShift()
        {
            _mocks = new MockRepository();
            _schedulePart = _mocks.StrictMock<IScheduleDay>();
            _target = new ShiftTradeChecksumCalculator(_schedulePart);

            var period = new DateTimePeriod(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2009, 1, 1, 9, 0, 0, DateTimeKind.Utc));
            IPersonAssignment personAssignment =
                PersonAssignmentFactory.CreateAssignmentWithPersonalShift(PersonFactory.CreatePerson(), period);

            using (_mocks.Record())
            {
                Expect.Call(_schedulePart.PersonAssignment()).Return(personAssignment);
            }

            using (_mocks.Playback())
            {
                long checksum1 = _target.CalculateChecksum();

                checksum1.Should().Be.EqualTo(-1);
            }
        }

        [Test]
        public void VerifyChecksumIsNotSameForChangedPersonAssignment()
        {
            _mocks = new MockRepository();
            _schedulePart = _mocks.StrictMock<IScheduleDay>();
            _target = new ShiftTradeChecksumCalculator(_schedulePart);

            long checksum1;
            long checksum2;

            var period1 = new DateTimePeriod(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2009, 1, 1, 9, 0, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2009, 1, 1, 9, 30, 0, DateTimeKind.Utc));
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IPerson person = PersonFactory.CreatePerson();
            IActivity activity = ActivityFactory.CreateActivity("activity1");
            activity.SetId(Guid.NewGuid());
            IShiftCategory schiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftcat1");

            IPersonAssignment personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period1, schiftCategory);
            IPersonAssignment personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, period2, schiftCategory);

            using (_mocks.Record())
            {
	            Expect.Call(_schedulePart.PersonAssignment()).Return(personAssignment1);
            }

            using (_mocks.Playback())
            {
                checksum1 = _target.CalculateChecksum();
            }

            _mocks = new MockRepository();
            _schedulePart = _mocks.StrictMock<IScheduleDay>();
            _target = new ShiftTradeChecksumCalculator(_schedulePart);
            
            using (_mocks.Record())
            {
                Expect.Call(_schedulePart.PersonAssignment()).Return(personAssignment2).Repeat.Any();
            }

            using (_mocks.Playback())
            {
                checksum2 = _target.CalculateChecksum();
            }

            Assert.AreNotEqual(checksum1, checksum2);
        }
    }
}
