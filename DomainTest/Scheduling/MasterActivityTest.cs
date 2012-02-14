using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class MasterActivityTest
    {
        private IMasterActivity _target;

        [SetUp]
        public void Setup()
        {
            _target = new MasterActivity();
        }

        [Test]
        public void VerifyActivityList()
        {
            Assert.AreEqual(0, _target.ActivityCollection.Count);
            IList<IActivity> activityList = new List<IActivity>();
            activityList.Add(ActivityFactory.CreateActivity("Hej"));
            activityList.Add(ActivityFactory.CreateActivity("Hopp"));
            _target.UpdateActivityCollection(activityList);
            Assert.AreEqual(2, _target.ActivityCollection.Count);
            Assert.AreEqual("Hopp", _target.ActivityCollection[1].Description.Name);
        }

        [Test]
        public void InContractTimeShouldReturnTrue()
        {
            Assert.IsTrue(_target.InContractTime);
            _target.InContractTime = false;
            Assert.IsTrue(_target.InContractTime);
        }
    }
}