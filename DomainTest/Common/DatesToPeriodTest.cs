using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests DatesToPeriod
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 2008-10-01
    /// </remarks>
    [TestFixture]
    public class DatesToPeriodTest
    {
        private IList<DateOnly> dateCollection;
        private DatesToPeriod target;

        [SetUp]
        public void Setup()
        {
            dateCollection = new List<DateOnly>();
            var date = DateOnly.Today;
            //period 1
            dateCollection.Add(date);

            for (int t = 1; t < 3; t++)
            {
                date = date.AddDays(1);
                dateCollection.Add(date);
            }

            //period 2
            date = date.AddDays(3);
            dateCollection.Add(date);

            //period 3
            date = date.AddDays(2);
            dateCollection.Add(date);

            for (int t = 1; t < 3; t++)
            {
                date = date.AddDays(1);
                dateCollection.Add(date);
            }

            target = new DatesToPeriod();
        }

        [Test]
        public void VerifyPeriodCount()
        {
            int expectedValue = target.Convert(dateCollection).Count;
            int actualValue = 3;

            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
