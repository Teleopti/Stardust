using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class PersonMeetingByDateSorterTest
    {
        private PersonMeetingByDateSorter _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new PersonMeetingByDateSorter();
        }

        [Test]
        public void ShouldSortAccordingToLastUpdateTime()
        {
            var personMeeting1 = _mocks.StrictMock<IPersonMeeting>();
            var personMeeting2 = _mocks.StrictMock<IPersonMeeting>();
            var meeting1 = _mocks.StrictMock<IMeeting>();
            var meeting2 = _mocks.StrictMock<IMeeting>();
            using(_mocks.Record())
            {
                Expect.Call(personMeeting1.BelongsToMeeting).Return(meeting1);
                Expect.Call(personMeeting2.BelongsToMeeting).Return(meeting2);
                Expect.Call(meeting1.UpdatedOn).Return(new DateTime(2012, 10, 11));
                Expect.Call(meeting2.UpdatedOn).Return(new DateTime(2012, 10, 12));
            }
            using(_mocks.Playback())
            {
                Assert.That(_target.Compare(personMeeting1, personMeeting2), Is.EqualTo(-1));
            }
        }

        [Test]
        public void ShouldHaveDefaultSortingWhenNoUpdateTimeIsAvailable()
        {
            var personMeeting1 = _mocks.StrictMock<IPersonMeeting>();
            var personMeeting2 = _mocks.StrictMock<IPersonMeeting>();
            var meeting1 = _mocks.StrictMock<IMeeting>();
            var meeting2 = _mocks.StrictMock<IMeeting>();
            using (_mocks.Record())
            {
                Expect.Call(personMeeting1.BelongsToMeeting).Return(meeting1);
                Expect.Call(personMeeting2.BelongsToMeeting).Return(meeting2);
                Expect.Call(meeting1.UpdatedOn).Return(null);
                Expect.Call(meeting2.UpdatedOn).Return(null);
                Expect.Call(personMeeting1.Period).Return(new DateTimePeriod(new DateTime(2012, 10, 11,0,0,0, DateTimeKind.Utc),
                                                                             new DateTime(2012, 10, 12,0,0,0,DateTimeKind.Utc))); 
                Expect.Call(personMeeting2.Period).Return(new DateTimePeriod(new DateTime(2012, 10, 12, 0, 0, 0, DateTimeKind.Utc),
                                                                             new DateTime(2012, 10, 13, 0, 0, 0, DateTimeKind.Utc)));
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.Compare(personMeeting1, personMeeting2), Is.EqualTo(-1));
            }
        }
    }
}
