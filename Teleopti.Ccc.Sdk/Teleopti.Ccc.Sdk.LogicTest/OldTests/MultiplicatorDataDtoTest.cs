using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class MultiplicatorDataDtoTest
    {
        private MultiplicatorDataDto target;

        [SetUp]
        public void Setup()
        {
            target = new MultiplicatorDataDto();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(DateTime.MinValue,target.Date);
            Assert.AreEqual(DateTime.MinValue,target.ActualDate);
            Assert.AreEqual(TimeSpan.Zero,target.Amount);
            Assert.IsNull(target.Multiplicator);

            target.Amount = TimeSpan.FromHours(1.5d);
            target.Date = DateTime.Today;
            target.ActualDate = DateTime.Today.AddDays(1);
			//target.Multiplicator =
			//    new MultiplicatorDto(MultiplicatorFactory.CreateMultiplicator("test", "", Color.FloralWhite,
			//                                                                  MultiplicatorType.Overtime, 1));
			target.Multiplicator = new MultiplicatorDto();
            Assert.AreEqual(DateTime.Today, target.Date);
            Assert.AreEqual(DateTime.Today.AddDays(1), target.ActualDate);
            Assert.AreEqual(TimeSpan.FromHours(1.5d), target.Amount);
        }
    }
}