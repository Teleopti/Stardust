using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    public class AbsenceRequestInfoTest
    {
        private AbsenceRequestInfo _target;

        [SetUp]
        public void Setup()
        {
            _target = new AbsenceRequestInfo();
        }

        [Test]
        public void GetAndSetNumberOfRequests()
        {
            _target.NumberOfRequests = 1;
            Assert.AreEqual(_target.NumberOfRequests , 1);
        }

        [Test]
        public void GetAndSetBelongsToDate()
        {
            _target.BelongsToDate = new DateTime(2013,01,01);
            Assert.AreEqual(_target.BelongsToDate, new DateTime(2013,01,01));
        }
    }
}
