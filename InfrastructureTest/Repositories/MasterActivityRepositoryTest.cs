using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests MAsterActivityRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class MasterActivityRepositoryTest : RepositoryTest<IMasterActivity>
    {
        private IActivity _activity1;
        private IActivity _activity2;
        private IActivity _activity3;

        protected override void ConcreteSetup()
        {

						_activity1 = new Activity("zz");
						_activity2 = new Activity("ff");
						_activity3 = new Activity("aa");


            PersistAndRemoveFromUnitOfWork(_activity1);
            PersistAndRemoveFromUnitOfWork(_activity2);
            PersistAndRemoveFromUnitOfWork(_activity3);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IMasterActivity CreateAggregateWithCorrectBusinessUnit()
        {
            IMasterActivity act = new MasterActivity
                                      {
                                          Description = new Description("NYMASTER", "NM"),
                                          DisplayColor = Color.DeepPink
                                      };
            act.UpdateActivityCollection(new List<IActivity> { _activity1, _activity2, _activity3 });

            return act;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IMasterActivity loadedAggregateFromDatabase)
        {
            if (loadedAggregateFromDatabase == null) throw new ArgumentNullException("loadedAggregateFromDatabase");
            IMasterActivity org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(org.DisplayColor.ToArgb(), loadedAggregateFromDatabase.DisplayColor.ToArgb());

            Assert.That(org.ActivityCollection.Count, Is.EqualTo(loadedAggregateFromDatabase.ActivityCollection.Count));

            Assert.That(org.ActivityCollection[0].Name, Is.EqualTo(loadedAggregateFromDatabase.ActivityCollection[0].Name));
        }
        [Test]
        public void VerifyCanPersistProperties()
        {
            IMasterActivity masterActivity = CreateAggregateWithCorrectBusinessUnit();
            masterActivity.DisplayColor = Color.DeepPink;
            masterActivity.Description = new Description("NYMASTER", "NM");
            PersistAndRemoveFromUnitOfWork(masterActivity);
            IMasterActivity loadedAbctivity = new MasterActivityRepository(UnitOfWork).Load(masterActivity.Id.Value);
            Assert.That(loadedAbctivity.DisplayColor.ToArgb(), Is.EqualTo(Color.DeepPink.ToArgb()));
            Assert.That(loadedAbctivity.Description.Name, Is.EqualTo("NYMASTER"));
            Assert.That(loadedAbctivity.Description.ShortName, Is.EqualTo("NM"));
        }

        protected override Repository<IMasterActivity> TestRepository(IUnitOfWork unitOfWork)
        {
            return new MasterActivityRepository(unitOfWork);
        }
    }
}
