using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class DateOnlyDtoTest
    {
        [Test]
        public void CanCreateInstance()
		{
			var dateOnly = DateOnly.Today;
			var target = new DateOnlyDto { DateTime = dateOnly.Date };

			Assert.IsNotNull(target);
            Assert.AreEqual(dateOnly.Date.Ticks, target.DateTime.Ticks);

            dateOnly = new DateOnly(2010, 8, 1);
            target = new DateOnlyDto(dateOnly.Year, dateOnly.Month, dateOnly.Day);
            Assert.AreEqual(dateOnly.Date.Ticks, target.DateTime.Ticks);
        }
    }
}