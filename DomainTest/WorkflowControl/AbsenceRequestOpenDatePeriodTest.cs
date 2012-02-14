using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class AbsenceRequestOpenDatePeriodTest : AbsenceRequestOpenPeriodTest
    {


        [Test]
        public void CanSetAndGetPeriod()
        {
            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2010, 06, 01, 2010, 08, 31);
            AbsenceRequestOpenDatePeriod absenceRequestOpenDatePeriod = (AbsenceRequestOpenDatePeriod) Target;
            absenceRequestOpenDatePeriod.Period = dateOnlyPeriod;
            Assert.AreEqual(dateOnlyPeriod, Target.GetPeriod(new DateOnly(DateTime.Today)));
            Assert.AreEqual(dateOnlyPeriod, absenceRequestOpenDatePeriod.Period);
        }



        protected override IAbsenceRequestOpenPeriod CreateInstance()
        {
            return new AbsenceRequestOpenDatePeriod();
        }

    }
}
