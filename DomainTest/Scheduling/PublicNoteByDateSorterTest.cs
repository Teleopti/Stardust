using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class PublicNoteByDateSorterTest
    {
        private PublicNoteByDateSorter _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _target = new PublicNoteByDateSorter();
            _mocks = new MockRepository();
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanCompare()
        {
            DateOnly first = new DateOnly(2010, 1, 1);
            DateOnly second = new DateOnly(2010, 1, 2);
            DateOnly third = new DateOnly(2010, 1, 3);

            IPublicNote note1 = _mocks.StrictMock<IPublicNote>();
            IPublicNote note2 = _mocks.StrictMock<IPublicNote>();
            IPublicNote note3 = _mocks.StrictMock<IPublicNote>();

            using (_mocks.Record())
            {
                Expect.Call(note1.NoteDate).Return(first).Repeat.Any();
                Expect.Call(note2.NoteDate).Return(second).Repeat.Any();
                Expect.Call(note3.NoteDate).Return(third).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(-1 == _target.Compare(note1, note2));
                Assert.IsTrue(0 == _target.Compare(note2, note2));
                Assert.IsTrue(1 == _target.Compare(note3, note2));
            }
        }

        [Test]
        public void CanCompareNulls()
        {
            DateOnly second = new DateOnly(2010, 1, 2);

            IPublicNote note1 = null;
            IPublicNote note2 = _mocks.StrictMock<IPublicNote>();

            using (_mocks.Record())
            {
                Expect.Call(note2.NoteDate).Return(second).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(-1 == _target.Compare(note1, note2));
                Assert.IsTrue(1 == _target.Compare(note2, note1));
                Assert.IsTrue(0 == _target.Compare(null, null));
            }
        }
    
    }
}
