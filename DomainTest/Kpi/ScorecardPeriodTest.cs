using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Kpi
{
    [TestFixture, SetUICulture("en-US")]
    public class ScorecardPeriodTest
    {
        IScorecardPeriod _target;

        [SetUp]
        public void Setup()
        {
            _target = new ScorecardPeriod(1);
        }

        [Test]
        public void CanCreateScorecardPeriod()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(1, _target.Id);
            Assert.AreEqual("Week", _target.Name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CannotCreateScorecardPeriodWithAnIdHigherThanFour()
        {
            _target = new ScorecardPeriod(5);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CannotCreateScorecardPeriodWithAnIdLowerThanZero()
        {
            _target = new ScorecardPeriod(-1);
        }

        [Test]
        public void ListIsReturnedAndIsNotEmpty()
        {
            IEnumerable<IScorecardPeriod> lst = ScorecardPeriodService.ScorecardPeriodList();
            Assert.IsNotNull(lst);
            Assert.Greater(lst.Count(),0 );
        }
    }
}
