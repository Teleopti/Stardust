using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class ShiftTradeSwapDetailTest
    {
        private ShiftTradeSwapDetail _target;
        private IPerson _personFrom;
        private IPerson _personTo;
        private DateOnly _dateFrom;
        private DateOnly _dateTo;

        [SetUp]
        public void Setup()
        {
            _personFrom = PersonFactory.CreatePerson();
            _personTo = PersonFactory.CreatePerson();
            _dateFrom = new DateOnly(2009,9,23);
            _dateTo = new DateOnly(2009, 9, 21);
            _target = new ShiftTradeSwapDetail(_personFrom,_personTo,_dateFrom,_dateTo);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_personFrom,_target.PersonFrom);
            Assert.AreEqual(_personTo, _target.PersonTo);
            Assert.AreEqual(_dateFrom,_target.DateFrom);
            Assert.AreEqual(_dateTo, _target.DateTo);

            _target.ChecksumFrom = 29;
            _target.ChecksumTo = 13;

            Assert.AreEqual(29,_target.ChecksumFrom);
            Assert.AreEqual(13, _target.ChecksumTo);
        }

        [Test]
        public void VerifyCanSetAndGetScheduleParts()
        {
            MockRepository mockRepository = new MockRepository();
            IScheduleDay part1 = mockRepository.StrictMock<IScheduleDay>();
            IScheduleDay part2 = mockRepository.StrictMock<IScheduleDay>();

            Assert.IsNull(_target.SchedulePartFrom);
            Assert.IsNull(_target.SchedulePartTo);

            mockRepository.ReplayAll();
            _target.SchedulePartFrom = part1;
            _target.SchedulePartTo = part2;
            mockRepository.VerifyAll();

            Assert.AreEqual(part1,_target.SchedulePartFrom);
            Assert.AreEqual(part2, _target.SchedulePartTo);
        }
    }
}
