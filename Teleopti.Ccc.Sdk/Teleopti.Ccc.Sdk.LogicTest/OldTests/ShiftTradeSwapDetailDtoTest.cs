using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ShiftTradeSwapDetailDtoTest
    {
        private PersonDto _personFrom;
        private PersonDto _personTo;
        private DateOnlyDto _dateFrom;
        private DateOnlyDto _dateTo;
        private Guid _id;
        private ShiftTradeSwapDetailDto _target;
        private SchedulePartDto _schedulePartFrom;
        private SchedulePartDto _schedulePartTo;
        private int _checksumFrom;
        private int _checksumTo;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/31/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
			_dateFrom = new DateOnlyDto { DateTime = new DateTime(2008, 10, 1) };
			_dateTo = new DateOnlyDto { DateTime = new DateTime(2008, 10, 3) };

            _id = Guid.NewGuid();

            _personFrom = new PersonDto();
            _personFrom.Id = _id;

            _personTo = new PersonDto();
            _personTo.Id = _id;

            _checksumFrom = -3;
            _checksumTo = -7;

            _schedulePartTo = new SchedulePartDto();
            _schedulePartFrom = new SchedulePartDto();

            _target = new ShiftTradeSwapDetailDto
                          {
                              DateFrom = _dateFrom,
                              DateTo = _dateTo,
                              Id = _id,
                              PersonFrom = _personFrom,
                              PersonTo = _personTo,
                              SchedulePartFrom = _schedulePartFrom,
                              SchedulePartTo = _schedulePartTo,
                              ChecksumFrom = _checksumFrom,
                              ChecksumTo = _checksumTo
                          };
        }

        /// <summary>
        /// Verifies the can set and get properties.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/31/2008
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_dateFrom,_target.DateFrom);
            Assert.AreEqual(_dateTo, _target.DateTo);
            Assert.AreEqual(_personFrom, _target.PersonFrom);
            Assert.AreEqual(_personTo, _target.PersonTo);
            Assert.AreEqual(_schedulePartFrom, _target.SchedulePartFrom);
            Assert.AreEqual(_schedulePartTo, _target.SchedulePartTo);
            Assert.AreEqual(_id,_target.Id);
            Assert.AreEqual(_checksumFrom,_target.ChecksumFrom);
            Assert.AreEqual(_checksumTo, _target.ChecksumTo);
        }
    }
}