using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
    [TestFixture]
    public class CommonRootCollectionTest
    {
        private MockRepository _mocks;
        private CommonRootCollection<IActivity> _targetCollection;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        }

    
        [Test]
        public void VerifyAddsAllItemsFromRepositoryToList()
        {
            IRepository<IActivity> activityRepository = _mocks.StrictMock<IRepository<IActivity>>();
            IActivity act1 = ActivityFactory.CreateActivity("test");
            IActivity act2 = ActivityFactory.CreateActivity("test");
            IList<IActivity> activities = new List<IActivity>(){act1,act2};

            using (_mocks.Record())
            {
                Expect.Call(activityRepository.LoadAll()).Return(activities);
            }
            using (_mocks.Playback())
            {
                _targetCollection = new CommonRootCollection<IActivity>(activityRepository);
                Assert.IsTrue(_targetCollection.Contains(act1));
                Assert.IsTrue(_targetCollection.Contains(act2));
                Assert.IsTrue(_targetCollection.Count == 2);
            }
        }

    }

   
}
