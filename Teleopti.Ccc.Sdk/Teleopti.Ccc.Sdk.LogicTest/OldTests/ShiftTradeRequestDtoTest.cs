using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ShiftTradeRequestDtoTest
    {
        private ShiftTradeRequestDto    _target;
        private Guid _id;

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
            _id = Guid.NewGuid();
            _target = new ShiftTradeRequestDto();
        }

        [Test]
        public void VerifyProperties()
        {
            _target.Id = _id;
            Assert.AreEqual(_id, _target.Id);

            _target.ShiftTradeStatus = ShiftTradeStatusDto.OkByBothParts;
            Assert.AreEqual(ShiftTradeStatusDto.OkByBothParts, _target.ShiftTradeStatus);

            _target.TypeDescription = "ShiftTradeDescription";
            Assert.AreEqual("ShiftTradeDescription", _target.TypeDescription);

            Assert.AreEqual(0, _target.ShiftTradeSwapDetails.Count);
        }
    }
}