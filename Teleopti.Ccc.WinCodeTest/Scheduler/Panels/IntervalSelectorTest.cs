using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.Panels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Panels
{
    [TestFixture]
    public class IntervalSelectorTest
    {
        DateTime _baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc); 
        [Test]
        public void VerifyReturnsCorrectIntervalFromPeriod()
        {
           
             
            IntervalSelector selector = new IntervalSelector();
            Assert.AreEqual(TimeSpan.FromMinutes(5), selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime, _baseDateTime.Add(TimeSpan.Zero))));
            Assert.AreEqual(TimeSpan.FromMinutes(5),selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime,_baseDateTime.Add(TimeSpan.FromHours(3)))));
            Assert.AreEqual(TimeSpan.FromHours(1),selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime,_baseDateTime.Add(TimeSpan.FromHours(36)))));
            Assert.AreEqual(TimeSpan.FromHours(3),selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime,_baseDateTime.Add(TimeSpan.FromHours(108)))));
            Assert.AreEqual(TimeSpan.FromHours(6),selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime,_baseDateTime.Add(TimeSpan.FromHours(216)))));
            Assert.AreEqual(TimeSpan.FromDays(1),selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime,_baseDateTime.Add(TimeSpan.FromDays(36)))));
            Assert.AreEqual(TimeSpan.FromDays(7), selector.SuggestedTimeSpan(new DateTimePeriod(_baseDateTime, _baseDateTime.Add(TimeSpan.FromDays(36).Add(TimeSpan.FromMinutes(1))))));

        }
    }
}
